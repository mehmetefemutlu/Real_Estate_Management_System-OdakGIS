import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnChanges, OnDestroy, Output, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { loadAdministrativeBoundaries } from '../osm-vector-layers';

interface ParsedCoordinates {
  latitude: number;
  longitude: number;
}

@Component({
  selector: 'app-property-map',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './property-map.component.html',
  styleUrl: './property-map.component.scss'
})
export class PropertyMapComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input() coordinates: string | undefined = '';
  @Input() editable = false;
  @Input() cityName = '';
  @Input() districtName = '';
  @Input() neighbourhoodName = '';
  @Output() coordinatesChange = new EventEmitter<string | undefined>();
  @Output() areaChange = new EventEmitter<number>();
  @ViewChild('mapTarget') mapTarget!: ElementRef<HTMLDivElement>;

  selectedPoints = 0;
  mapError = '';
  showAdministrativeBoundaries = true;

  private map: any;
  private vectorSource: any;
  private boundarySource: any;
  private boundaryLayer: any;
  private ol: any;
  private points: number[][] = [];
  private overlayAbortController: AbortController | null = null;
  private overlayRequestId = 0;

  async ngAfterViewInit() {
    if (globalThis.window === undefined) return;

    try {
      const [
        { default: OlMap },
        { default: View },
        { default: TileLayer },
        { default: OSM },
        { fromLonLat, toLonLat },
        { default: Feature },
        { default: Point },
        { default: LineString },
        { default: Polygon },
        { default: VectorLayer },
        { default: VectorSource },
        { Style, Circle, Fill, Stroke },
        { getArea }
      ] = await Promise.all([
        import('ol/Map.js'),
        import('ol/View.js'),
        import('ol/layer/Tile.js'),
        import('ol/source/OSM.js'),
        import('ol/proj.js'),
        import('ol/Feature.js'),
        import('ol/geom/Point.js'),
        import('ol/geom/LineString.js'),
        import('ol/geom/Polygon.js'),
        import('ol/layer/Vector.js'),
        import('ol/source/Vector.js'),
        import('ol/style.js'),
        import('ol/sphere.js')
      ]);

      this.ol = { Map: OlMap, View, TileLayer, OSM, fromLonLat, toLonLat, Feature, Point, LineString, Polygon, VectorLayer, VectorSource, Style, Circle, Fill, Stroke, getArea };
      this.vectorSource = new VectorSource();
      this.boundarySource = new VectorSource();
      this.boundaryLayer = new VectorLayer({ source: this.boundarySource });

      this.map = new OlMap({
        target: this.mapTarget.nativeElement,
        layers: [
          new TileLayer({ source: new OSM({ crossOrigin: 'anonymous' }) }),
          this.boundaryLayer,
          new VectorLayer({ source: this.vectorSource })
        ],
        view: new View({
          center: fromLonLat([35.2433, 38.9637]),
          zoom: 6
        })
      });
      this.syncGeometry(true);
      this.applyLayerSettings();

      if (this.editable) {
        this.map.on('click', (event: any) => this.addPolygonPoint(event.coordinate));
      }
      setTimeout(() => this.map?.updateSize(), 0);
    } catch {
      this.mapError = 'Map could not be loaded.';
    }
  }
  ngOnChanges(changes: SimpleChanges) {
    if (changes['coordinates'] && this.map) {
      this.syncGeometry(true);
    }
  }
  ngOnDestroy() {
    this.overlayAbortController?.abort();
    this.map?.setTarget(undefined);
  }

  get hasCoordinates(): boolean {
    return !!this.parsePolygon(this.coordinates) || !!this.parsePoint(this.coordinates);
  }

  get isPolygonComplete(): boolean {
    return this.points.length === 4;
  }

  get boundaryLabel(): string {
    return [this.neighbourhoodName, this.districtName, this.cityName]
      .filter(Boolean)
      .join(' / ') || 'Administrative boundaries';
  }

  applyLayerSettings() {
    this.boundaryLayer?.setVisible(this.showAdministrativeBoundaries);
  }

  onBoundaryToggle(event: Event) {
    this.showAdministrativeBoundaries = (event.target as HTMLInputElement).checked;
    this.applyLayerSettings();
  }

  resetSelection() {
    this.points = [];
    this.selectedPoints = 0;
    this.coordinates = '';
    this.vectorSource?.clear();
    this.clearContextLayers();
    this.coordinatesChange.emit('');
    this.areaChange.emit(0);
  }

  private addPolygonPoint(coordinate: number[]) {
    if (!this.editable || this.points.length >= 4) return;

    this.points.push(coordinate);
    this.selectedPoints = this.points.length;
    this.redrawDraft();

    if (this.points.length === 4) {
      const ring = [...this.points, this.points[0]];
      const lonLatRing = ring.map(point => this.ol.toLonLat(point));
      const polygon = new this.ol.Polygon([ring]);
      const area = Math.round(this.ol.getArea(polygon));

      this.coordinates = JSON.stringify({
        type: 'Polygon',
        coordinates: [lonLatRing.map(([longitude, latitude]: number[]) => [
          Number(longitude.toFixed(6)),
          Number(latitude.toFixed(6))
        ])]
      });

      this.coordinatesChange.emit(this.coordinates);
      this.areaChange.emit(area);
      this.drawPolygon(ring, true);
      this.loadRealContextLayers(ring);
    }
  }

  private syncGeometry(animate: boolean) {
    if (!this.ol || !this.vectorSource) return;

    const polygon = this.parsePolygon(this.coordinates);
    if (polygon) {
      const ring = polygon.map(([longitude, latitude]) => this.ol.fromLonLat([longitude, latitude]));
      this.points = ring.slice(0, 4);
      this.selectedPoints = this.points.length;
      this.drawPolygon(ring, animate);
      this.loadRealContextLayers(ring);
      return;
    }

    const point = this.parsePoint(this.coordinates);
    if (point) {
      const coordinate = this.ol.fromLonLat([point.longitude, point.latitude]);
      this.points = [coordinate];
      this.selectedPoints = 1;
      this.vectorSource.clear();
      this.addPointFeature(coordinate);
      this.loadRealContextLayers([coordinate]);
      this.map.getView().animate({ center: coordinate, zoom: 15, duration: animate ? 220 : 0 });
      return;
    }

    this.points = [];
    this.selectedPoints = 0;
    this.vectorSource.clear();
    this.clearContextLayers();
  }

  private redrawDraft() {
    this.vectorSource.clear();
    this.clearContextLayers();
    this.points.forEach(point => this.addPointFeature(point));

    if (this.points.length > 1) {
      const line = new this.ol.Feature({ geometry: new this.ol.LineString(this.points) });
      line.setStyle(new this.ol.Style({
        stroke: new this.ol.Stroke({ color: '#2f6fed', width: 3 })
      }));
      this.vectorSource.addFeature(line);
    }
  }

  private drawPolygon(ring: number[][], animate: boolean) {
    this.vectorSource.clear();
    const polygonFeature = new this.ol.Feature({ geometry: new this.ol.Polygon([ring]) });
    polygonFeature.setStyle(new this.ol.Style({
      fill: new this.ol.Fill({ color: 'rgba(47, 111, 237, 0.18)' }),
      stroke: new this.ol.Stroke({ color: '#2f6fed', width: 3 })
    }));
    this.vectorSource.addFeature(polygonFeature);
    ring.slice(0, -1).forEach(point => this.addPointFeature(point));

    if (animate) {
      const extent = polygonFeature.getGeometry().getExtent();
      this.map.getView().fit(extent, { padding: [40, 40, 40, 40], maxZoom: 17, duration: 220 });
    }
  }

  private async loadRealContextLayers(projectedCoordinates: number[][]) {
    if (this.editable || !this.ol || !this.boundarySource || projectedCoordinates.length === 0) return;

    this.clearContextLayers();
    const xs = projectedCoordinates.map(point => point[0]);
    const ys = projectedCoordinates.map(point => point[1]);
    const extent = [Math.min(...xs), Math.min(...ys), Math.max(...xs), Math.max(...ys)];
    const requestId = ++this.overlayRequestId;

    this.overlayAbortController?.abort();
    this.overlayAbortController = new AbortController();

    try {
      await loadAdministrativeBoundaries(
        this.ol,
        extent,
        this.boundarySource,
        this.overlayAbortController.signal
      );
    } catch (error: any) {
      if (requestId === this.overlayRequestId && error?.name !== 'AbortError') {
        this.clearContextLayers();
      }
    }
  }

  private clearContextLayers() {
    this.overlayAbortController?.abort();
    this.boundarySource?.clear();
  }

  private addPointFeature(coordinate: number[]) {
    const marker = new this.ol.Feature({ geometry: new this.ol.Point(coordinate) });
    marker.setStyle(new this.ol.Style({
      image: new this.ol.Circle({
        radius: 6,
        fill: new this.ol.Fill({ color: '#c67a1c' }),
        stroke: new this.ol.Stroke({ color: '#ffffff', width: 3 })
      })
    }));
    this.vectorSource.addFeature(marker);
  }

  private parsePolygon(value: string | undefined): number[][] | null {
    if (!value?.trim()) return null;

    const rawPoints = this.parseCoordinatePairs(value);
    if (rawPoints.length >= 4) return rawPoints;

    try {
      const parsed = JSON.parse(value);
      const coordinates = parsed?.type === 'Polygon' ? parsed.coordinates?.[0] : null;
      if (!Array.isArray(coordinates) || coordinates.length < 4) return null;
      return coordinates
        .map((point: unknown) => Array.isArray(point) ? [Number(point[0]), Number(point[1])] : null)
        .filter((point: number[] | null): point is number[] =>
          !!point && Number.isFinite(point[0]) && Number.isFinite(point[1]));
    } catch {
      return null;
    }
  }

  private parsePoint(value: string | undefined): ParsedCoordinates | null {
    if (!value?.trim()) return null;

    const parts = value.split(/[,\s]+/).map(Number).filter(Number.isFinite);
    if (parts.length < 2) return null;

    const [first, second] = parts;
    if (Math.abs(first) <= 90 && Math.abs(second) <= 180) {
      return { latitude: first, longitude: second };
    }

    if (Math.abs(first) <= 180 && Math.abs(second) <= 90) {
      return { latitude: second, longitude: first };
    }

    return null;
  }

  private parseCoordinatePairs(value: string): number[][] {
    const matches = [...value.matchAll(/\[\s*(-?\d+(?:[.,]\d+)?)\s*,\s*(-?\d+(?:[.,]\d+)?)\s*\]/g)];
    return matches
      .map(match => [Number(match[1].replace(',', '.')), Number(match[2].replace(',', '.'))])
      .filter(point => Number.isFinite(point[0]) && Number.isFinite(point[1]));
  }
}
