import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, Input, NgZone, OnChanges, OnDestroy, SimpleChanges, ViewChild, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ReactiveFormsModule, UntypedFormBuilder } from '@angular/forms';
import { CommonModule, DecimalPipe } from '@angular/common';
import { PropertyType, PropertyView } from '../../../models/property.model';
import { loadAdministrativeBoundaries, shouldLoadOsmOverlays } from '../osm-vector-layers';

type BaseLayerKey = 'osm' | 'google';

interface MapPoint {
  id: number;
  title: string;
  price: number;
  address: string;
  area: number;
  parcelNumber: string;
  lotNumber: string;
  propertyType: PropertyType;
  cityName: string;
  districtName: string;
  longitude: number;
  latitude: number;
}

@Component({
  selector: 'app-property-overview-map',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DecimalPipe],
  templateUrl: './property-overview-map.component.html',
  styleUrl: './property-overview-map.component.scss'
})
export class PropertyOverviewMapComponent implements AfterViewInit, OnChanges, OnDestroy {
  private readonly fb = inject(UntypedFormBuilder);
  @Input() properties: PropertyView[] = [];
  @ViewChild('mapTarget') mapTarget!: ElementRef<HTMLDivElement>;

  baseLayer: BaseLayerKey = 'osm';
  overlayOpacity = 55;
  showAdministrativeBoundaries = true;
  mapControlForm = this.fb.group({
    baseLayer: [this.baseLayer],
    showAdministrativeBoundaries: [this.showAdministrativeBoundaries],
    overlayOpacity: [this.overlayOpacity]
  });
  selectedProperty: MapPoint | null = null;
  mapError = '';

  private map: any;
  private osmLayer: any;
  private googleLayer: any;
  private markerLayer: any;
  private markerSource: any;
  private boundarySource: any;
  private boundaryLayer: any;
  private ol: any;
  private overlayAbortController: AbortController | null = null;
  private overlayLoadTimer: ReturnType<typeof setTimeout> | undefined;
  private overlayRequestId = 0;

  constructor(
    private readonly router: Router,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {}

  async ngAfterViewInit() {
    if (globalThis.window === undefined) return;

    try {
      const [
        { default: OlMap },
        { default: View },
        { default: TileLayer },
        { default: OSM },
        { default: XYZ },
        { default: Feature },
        { default: Point },
        { default: LineString },
        { default: Polygon },
        { default: VectorLayer },
        { default: VectorSource },
        { Style, Circle, Fill, Stroke },
        { fromLonLat, toLonLat },
        { boundingExtent },
        { default: ScaleLine }
      ] = await Promise.all([
        import('ol/Map.js'),
        import('ol/View.js'),
        import('ol/layer/Tile.js'),
        import('ol/source/OSM.js'),
        import('ol/source/XYZ.js'),
        import('ol/Feature.js'),
        import('ol/geom/Point.js'),
        import('ol/geom/LineString.js'),
        import('ol/geom/Polygon.js'),
        import('ol/layer/Vector.js'),
        import('ol/source/Vector.js'),
        import('ol/style.js'),
        import('ol/proj.js'),
        import('ol/extent.js'),
        import('ol/control/ScaleLine.js')
      ]);

      this.ol = { Map: OlMap, View, TileLayer, OSM, XYZ, Feature, Point, LineString, Polygon, VectorLayer, VectorSource, Style, Circle, Fill, Stroke, fromLonLat, toLonLat, boundingExtent, ScaleLine };
      this.markerSource = new VectorSource();
      this.boundarySource = new VectorSource();
      this.osmLayer = new TileLayer({ source: new OSM({ crossOrigin: 'anonymous' }) });
      this.googleLayer = new TileLayer({
        source: new XYZ({
          url: 'https://mt1.google.com/vt/lyrs=r&x={x}&y={y}&z={z}',
          crossOrigin: 'anonymous'
        }),
        visible: false
      });

      this.boundaryLayer = new VectorLayer({ source: this.boundarySource });
      this.markerLayer = new VectorLayer({ source: this.markerSource });

      this.map = new OlMap({
        target: this.mapTarget.nativeElement,
        layers: [
          this.osmLayer,
          this.googleLayer,
          this.boundaryLayer,
          this.markerLayer
        ],
        controls: [new ScaleLine({ bar: true, text: true, minWidth: 120 })],
        view: new View({
          center: fromLonLat([35.2433, 38.9637]),
          zoom: 6
        })
      });

      this.map.on('click', (event: any) => this.handleMapClick(event.pixel));
      this.map.on('pointermove', (event: any) => this.updatePointerCursor(event.pixel));
      this.map.on('moveend', () => this.queueOverlayLoad());
      this.renderProperties(true);
      this.applyLayerSettings();
      setTimeout(() => this.map?.updateSize(), 0);
    } catch {
      this.mapError = 'Map could not be loaded.';
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['properties'] && this.map) {
      this.renderProperties(true);
    }
  }

  ngOnDestroy() {
    this.overlayAbortController?.abort();
    if (this.overlayLoadTimer) globalThis.clearTimeout(this.overlayLoadTimer);
    this.map?.setTarget(undefined);
  }

  applyLayerSettings() {
    this.osmLayer?.setVisible(this.baseLayer === 'osm');
    this.googleLayer?.setVisible(this.baseLayer === 'google');
    this.boundaryLayer?.setVisible(this.showAdministrativeBoundaries);
    this.boundaryLayer?.setOpacity(this.overlayOpacity / 100);
  }

  onBaseLayerChange() {
    this.baseLayer = this.mapControlForm.get('baseLayer')?.value as BaseLayerKey;
    this.applyLayerSettings();
  }

  onBoundaryToggle() {
    this.showAdministrativeBoundaries = !!this.mapControlForm.get('showAdministrativeBoundaries')?.value;
    this.applyLayerSettings();
    this.queueOverlayLoad();
  }

  onOverlayOpacityChange() {
    this.overlayOpacity = Number(this.mapControlForm.get('overlayOpacity')?.value) || 55;
    this.applyLayerSettings();
  }

  openSelectedProperty() {
    if (this.selectedProperty) {
      this.router.navigate(['/properties', this.selectedProperty.id]);
    }
  }

  closePopup() {
    this.selectedProperty = null;
    this.refresh();
  }

  getTypeName(type: PropertyType): string {
    return PropertyType[type] ?? 'Unknown';
  }

  private renderProperties(fitMap: boolean) {
    if (!this.ol || !this.markerSource) return;

    this.markerSource.clear();
    this.clearOverlaySources();
    this.selectedProperty = null;

    const points = this.properties
      .map(property => this.toMapPoint(property))
      .filter((point): point is MapPoint => !!point);

    for (const point of points) {
      const feature = new this.ol.Feature({
        geometry: new this.ol.Point(this.ol.fromLonLat([point.longitude, point.latitude])),
        property: point
      });
      feature.setStyle(new this.ol.Style({
        image: new this.ol.Circle({
          radius: 7,
          fill: new this.ol.Fill({ color: '#c67a1c' }),
          stroke: new this.ol.Stroke({ color: '#ffffff', width: 3 })
        })
      }));
      this.markerSource.addFeature(feature);
    }

    if (fitMap && points.length > 0) {
      const projected = points.map(point => this.ol.fromLonLat([point.longitude, point.latitude]));
      const extent = this.ol.boundingExtent(projected);
      this.map.getView().fit(extent, { padding: [42, 42, 42, 42], maxZoom: 15, duration: 220 });
    }

    this.queueOverlayLoad();
  }

  private handleMapClick(pixel: number[]) {
    this.zone.run(() => {
      const property = this.getPropertyAtPixel(pixel);
      if (!property) {
        this.selectedProperty = null;
        this.refresh();
        return;
      }

      this.selectedProperty = property;
      this.map.getView().animate({
        center: this.ol.fromLonLat([property.longitude, property.latitude]),
        duration: 180
      });
      this.refresh();
    });
  }

  private getPropertyAtPixel(pixel: number[]): MapPoint | null {
    const feature = this.map.forEachFeatureAtPixel(
      pixel,
      (hit: any) => hit?.get?.('property') ? hit : undefined,
      {
        hitTolerance: 12,
        layerFilter: (layer: any) => layer === this.markerLayer
      }
    );
    return feature?.get?.('property') ?? null;
  }

  private updatePointerCursor(pixel: number[]) {
    if (!this.mapTarget?.nativeElement || !this.map) return;
    const hasMarker = this.map.hasFeatureAtPixel(pixel, {
      hitTolerance: 12,
      layerFilter: (layer: any) => layer === this.markerLayer
    });
    this.mapTarget.nativeElement.style.cursor = hasMarker ? 'pointer' : '';
  }

  private refresh() {
    this.cdr.detectChanges();
  }

  queueOverlayLoad() {
    if (!this.map || !this.ol || !this.boundarySource) return;
    if (this.overlayLoadTimer) globalThis.clearTimeout(this.overlayLoadTimer);
    this.overlayLoadTimer = globalThis.setTimeout(() => this.loadRealOverlaysForView(), 350);
  }

  private async loadRealOverlaysForView() {
    if (!this.map || !this.ol || !this.boundarySource) return;

    if (!this.showAdministrativeBoundaries || !shouldLoadOsmOverlays(this.map)) {
      this.clearOverlaySources();
      return;
    }

    const size = this.map.getSize();
    if (!size) return;

    const requestId = ++this.overlayRequestId;
    const extent = this.map.getView().calculateExtent(size);
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
        this.clearOverlaySources();
      }
    }
  }

  private clearOverlaySources() {
    this.overlayAbortController?.abort();
    this.boundarySource?.clear();
  }

  private toMapPoint(property: PropertyView): MapPoint | null {
    const center = this.parseCenter(property.coordinates);
    if (!center) return null;
    return {
      id: property.propertyID,
      title: property.title,
      price: property.price,
      address: property.address,
      area: property.area,
      parcelNumber: property.parcelNumber,
      lotNumber: property.lotNumber,
      propertyType: property.propertyType,
      cityName: property.cityName,
      districtName: property.districtName,
      longitude: center.longitude,
      latitude: center.latitude
    };
  }

  private parseCenter(value: string | undefined): { longitude: number; latitude: number } | null {
    if (!value?.trim()) return null;

    const rawPoints = this.parseCoordinatePairs(value);
    if (rawPoints.length > 0) return this.getCenter(rawPoints);

    const polygonCenter = this.parseGeoJsonPolygonCenter(value);
    if (polygonCenter) return polygonCenter;

    return this.parseLatitudeLongitudeCenter(value);
  }

  private parseLatitudeLongitudeCenter(value: string): { longitude: number; latitude: number } | null {
    const parts = value.split(/[,\s]+/).map(Number).filter(Number.isFinite);
    if (parts.length < 2) return null;

    const [first, second] = parts;
    if (Math.abs(first) <= 90 && Math.abs(second) <= 180) return { longitude: second, latitude: first };
    if (Math.abs(first) <= 180 && Math.abs(second) <= 90) return { longitude: first, latitude: second };
    return null;
  }

  private parseGeoJsonPolygonCenter(value: string): { longitude: number; latitude: number } | null {
    try {
      const parsed = JSON.parse(value);
      const coordinates = parsed?.type === 'Polygon' ? parsed.coordinates?.[0] : null;
      return this.getCoordinateCenter(coordinates);
    } catch {
      return null;
    }
  }

  private getCoordinateCenter(coordinates: unknown): { longitude: number; latitude: number } | null {
    if (!Array.isArray(coordinates) || coordinates.length === 0) return null;

    const points = coordinates
      .map((point: unknown) => Array.isArray(point) ? [Number(point[0]), Number(point[1])] : null)
      .filter((point: number[] | null): point is number[] =>
        !!point && Number.isFinite(point[0]) && Number.isFinite(point[1]));

    return points.length > 0 ? this.getCenter(points) : null;
  }

  private parseCoordinatePairs(value: string): number[][] {
    const matches = [...value.matchAll(/\[\s*(-?\d+(?:[.,]\d+)?)\s*,\s*(-?\d+(?:[.,]\d+)?)\s*\]/g)];
    return matches
      .map(match => [Number(match[1].replace(',', '.')), Number(match[2].replace(',', '.'))])
      .filter(point => Number.isFinite(point[0]) && Number.isFinite(point[1]));
  }

  private getCenter(points: number[][]): { longitude: number; latitude: number } {
    const uniquePoints = points.length > 1 &&
      points[0][0] === points.at(-1)![0] &&
      points[0][1] === points.at(-1)![1]
      ? points.slice(0, -1)
      : points;
    const totals = uniquePoints.reduce(
      (sum, point) => ({ longitude: sum.longitude + point[0], latitude: sum.latitude + point[1] }),
      { longitude: 0, latitude: 0 }
    );
    return { longitude: totals.longitude / uniquePoints.length, latitude: totals.latitude / uniquePoints.length };
  }
}
