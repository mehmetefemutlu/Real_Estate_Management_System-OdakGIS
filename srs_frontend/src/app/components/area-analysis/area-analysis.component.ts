import { CommonModule, DecimalPipe } from '@angular/common';
import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, ViewChild, inject } from '@angular/core';
import { ReactiveFormsModule, UntypedFormBuilder } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { StoredPolygon } from '../../models/polygon.model';
import { PolygonService } from './area-analysis.service';

type InputLabel = '1' | '2' | '3';
type GeometryLabel = InputLabel | '4' | '5';
type SelectionMode = 'manual' | 'auto';
type OperationKey = 'a-intersect-b' | 'b-intersect-a' | 'a-union-b' | 'a-union-b-union-c';

interface AnalysisPolygon {
  label: InputLabel;
  coordinates: number[][];
  area: number;
}

interface OperationResult {
  title: string;
  coordinates: number[][];
  geometryJson: string;
  area: number;
  savedLabel?: GeometryLabel;
  isEmpty: boolean;
}

@Component({
  selector: 'app-area-analysis',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DecimalPipe],
  templateUrl: './area-analysis.component.html',
  styleUrl: './area-analysis.component.scss'
})
export class AreaAnalysisComponent implements AfterViewInit, OnDestroy {
  private readonly fb = inject(UntypedFormBuilder);
  @ViewChild('mapTarget') mapTarget!: ElementRef<HTMLDivElement>;

  mode: SelectionMode = 'manual';
  polygons: Partial<Record<GeometryLabel, AnalysisPolygon>> = {};
  activeDrawLabel: InputLabel = '1';
  message = '';
  error = '';
  result: OperationResult | null = null;
  loading = false;
  mapError = '';
  savedPolygons: StoredPolygon[] = [];
  selectedPolygonIds: Partial<Record<InputLabel, number | null>> = {};
  selectedPolygonForm = this.fb.group({
    1: [null],
    2: [null],
    3: [null]
  });

  readonly drawLabels: InputLabel[] = ['1', '2', '3'];
  readonly operations: Array<{ key: OperationKey; label: string }> = [
    { key: 'a-intersect-b', label: 'Compute 1 ∩ 2' },
    { key: 'b-intersect-a', label: 'Compute 2 ∩ 1' },
    { key: 'a-union-b', label: 'Compute 1 ∪ 2' },
    { key: 'a-union-b-union-c', label: 'Compute 1 ∪ 2 ∪ 3' }
  ];

  private map: any;
  private ol: any;
  private source: any;
  private drawInteraction: any;
  private runIdSequence = 0;

  constructor(
    private readonly polygonService: PolygonService,
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
        { default: Feature },
        { default: Polygon },
        { default: VectorLayer },
        { default: VectorSource },
        { Style, Fill, Stroke, Text },
        { default: Draw },
        { fromLonLat, toLonLat },
        { boundingExtent }
      ] = await Promise.all([
        import('ol/Map.js'),
        import('ol/View.js'),
        import('ol/layer/Tile.js'),
        import('ol/source/OSM.js'),
        import('ol/Feature.js'),
        import('ol/geom/Polygon.js'),
        import('ol/layer/Vector.js'),
        import('ol/source/Vector.js'),
        import('ol/style.js'),
        import('ol/interaction/Draw.js'),
        import('ol/proj.js'),
        import('ol/extent.js')
      ]);

      this.ol = { Map: OlMap, View, TileLayer, OSM, Feature, Polygon, VectorLayer, VectorSource, Style, Fill, Stroke, Text, Draw, fromLonLat, toLonLat, boundingExtent };
      this.source = new VectorSource();
      const vectorLayer = new VectorLayer({ source: this.source });

      this.map = new OlMap({
        target: this.mapTarget.nativeElement,
        layers: [
          new TileLayer({ source: new OSM({ crossOrigin: 'anonymous' }) }),
          vectorLayer
        ],
        view: new View({
          center: fromLonLat([32.8597, 39.9334]),
          zoom: 10
        })
      });

      this.enableManualDraw();
      setTimeout(() => this.map?.updateSize(), 0);
    } catch {
      this.mapError = 'Map could not be loaded.';
    }
  }

  ngOnDestroy() {
    if (this.drawInteraction) this.map?.removeInteraction(this.drawInteraction);
    this.map?.setTarget(undefined);
  }

  setMode(mode: SelectionMode) {
    this.mode = mode;
    this.error = '';
    this.message = '';
    if (mode === 'manual') {
      this.enableManualDraw();
    } else {
      this.disableDraw();
    }
  }

  selectDrawLabel(label: InputLabel) {
    this.activeDrawLabel = label;
  }

  async autoSelect() {
    this.setMode('auto');
    this.loading = true;
    this.error = '';
    this.message = '';
    this.result = null;
    this.refresh();

    try {
      this.savedPolygons = await firstValueFrom(this.polygonService.getPolygons());
      const usable = this.savedPolygons.filter(item => this.parsePolygonCoordinates(item.coordinates).length >= 4);

      if (usable.length < 3) {
        this.polygons = {};
        this.renderAll();
        this.message = 'No saved geometries found. Please use Manual Draw.';
        return;
      }

      this.selectedPolygonIds = {
        1: usable[0].id,
        2: usable[1].id,
        3: usable[2].id
      };
      this.selectedPolygonForm.reset(this.selectedPolygonIds);
      this.applySelectedPolygons();
      this.message = 'Saved polygons loaded. Choose any three for 1, 2, and 3.';
    } catch (err: any) {
      this.error = err?.error?.message || err?.message || 'Saved geometries could not be loaded.';
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  applySelectedPolygons() {
    this.error = '';
    this.message = '';
    const usedIds = new Set<number>();
    const next: Partial<Record<GeometryLabel, AnalysisPolygon>> = {};
    this.selectedPolygonIds = this.selectedPolygonForm.getRawValue() as Partial<Record<InputLabel, number | null>>;

    for (const label of this.drawLabels) {
      const id = Number(this.selectedPolygonIds[label]);
      if (!id) {
        this.error = 'Please select three saved polygons.';
        return;
      }

      if (usedIds.has(id)) {
        this.error = 'Please select three different polygons.';
        return;
      }

      const stored = this.savedPolygons.find(polygon => polygon.id === id);
      const coordinates = stored ? this.parsePolygonCoordinates(stored.coordinates) : [];
      if (!stored || coordinates.length < 4) {
        this.error = 'Selected polygon could not be loaded.';
        return;
      }

      usedIds.add(id);
      next[label] = {
        label,
        coordinates,
        area: this.calculateArea(coordinates)
      };
    }

    this.polygons = next;
    this.result = null;
    this.renderAll(true);
    this.message = 'Selected polygons are ready for analysis.';
    this.refresh();
  }

  async saveManualGeometries(auto = false) {
    this.error = '';
    this.message = '';

    if (!this.polygons['1'] || !this.polygons['2'] || !this.polygons['3']) {
      this.error = 'Please complete geometries 1, 2, and 3.';
      return;
    }

    this.loading = true;
    this.refresh();

    try {
      const runId = this.getRunId();
      await firstValueFrom(this.polygonService.createPolygons({
        polygons: this.drawLabels.map(label => ({
          label: `${label}-${runId}`,
          coordinates: this.toPolygonJson(this.polygons[label]!.coordinates)
        }))
      }));
      this.message = auto ? 'Geometries 1, 2, and 3 saved automatically.' : 'Geometries 1, 2, and 3 saved.';
    } catch (err: any) {
      this.error = err?.error?.message || err?.message || 'Geometries could not be saved.';
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  async runOperation(operation: OperationKey) {
    this.error = '';
    this.message = '';
    this.result = null;

    if (!this.hasRequiredOperationPolygons(operation)) {
      this.error = 'Please complete geometries 1, 2, and 3.';
      return;
    }

    if (operation === 'a-intersect-b' || operation === 'b-intersect-a') {
      this.runIntersectionOperation(operation);
      return;
    }

    await this.runUnionOperation(operation);
  }

  private hasRequiredOperationPolygons(operation: OperationKey): boolean {
    return !!this.polygons['1'] && !!this.polygons['2'] && (operation !== 'a-union-b-union-c' || !!this.polygons['3']);
  }

  private runIntersectionOperation(operation: 'a-intersect-b' | 'b-intersect-a') {
    const first = operation === 'a-intersect-b' ? this.polygons['1']!.coordinates : this.polygons['2']!.coordinates;
    const second = operation === 'a-intersect-b' ? this.polygons['2']!.coordinates : this.polygons['1']!.coordinates;
    const title = operation === 'a-intersect-b' ? '1 ∩ 2' : '2 ∩ 1';
    const intersection = this.intersectPolygons(first, second);

    if (intersection.length < 4) {
      this.result = this.createEmptyOperationResult(title);
      this.renderAll();
      this.message = 'No intersection found.';
      return;
    }

    this.result = {
      title,
      coordinates: intersection,
      geometryJson: this.toPolygonJson(intersection),
      area: this.calculateArea(intersection),
      isEmpty: false
    };
    this.renderAll(true);
  }

  private createEmptyOperationResult(title: string): OperationResult {
    return {
      title,
      coordinates: [],
      geometryJson: '',
      area: 0,
      isEmpty: true
    };
  }

  private async runUnionOperation(operation: 'a-union-b' | 'a-union-b-union-c') {
    const resultLabel: GeometryLabel = operation === 'a-union-b' ? '4' : '5';
    const sourcePolygons = operation === 'a-union-b'
      ? [this.polygons['1']!, this.polygons['2']!]
      : [this.polygons['1']!, this.polygons['2']!, this.polygons['3']!];
    const coordinates = sourcePolygons.map(polygon => this.closeRing(polygon.coordinates));
    const area = this.calculateUnionArea(sourcePolygons.map(polygon => polygon.coordinates));
    const geometryJson = this.toMultiPolygonJson(coordinates);

    this.result = {
      title: operation === 'a-union-b' ? '1 ∪ 2' : '1 ∪ 2 ∪ 3',
      coordinates: [],
      geometryJson,
      area,
      savedLabel: resultLabel,
      isEmpty: false
    };

    this.loading = true;
    this.refresh();
    try {
      await firstValueFrom(this.polygonService.createPolygons({
        polygons: [{ label: `${resultLabel}-${this.getRunId()}`, coordinates: geometryJson }]
      }));
      this.message = `Geometry ${resultLabel} saved as a new polygon.`;
      this.renderAll(true);
    } catch (err: any) {
      this.error = err?.error?.message || err?.message || `Geometry ${resultLabel} could not be saved.`;
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  clear() {
    this.polygons = {};
    this.result = null;
    this.error = '';
    this.message = '';
    this.activeDrawLabel = '1';
    this.selectedPolygonIds = {};
    this.selectedPolygonForm.reset({ 1: null, 2: null, 3: null });
    this.source?.clear();
    this.setMode('manual');
  }

  hasAllInputs(): boolean {
    return !!this.polygons['1'] && !!this.polygons['2'] && !!this.polygons['3'];
  }

  private enableManualDraw() {
    if (!this.map || !this.ol) return;
    this.disableDraw();
    this.drawInteraction = new this.ol.Draw({
      source: this.source,
      type: 'Polygon'
    });
    this.drawInteraction.on('drawend', (event: any) => this.captureDrawnPolygon(event.feature));
    this.map.addInteraction(this.drawInteraction);
  }

  private disableDraw() {
    if (this.drawInteraction) {
      this.map?.removeInteraction(this.drawInteraction);
      this.drawInteraction = null;
    }
  }

  private captureDrawnPolygon(feature: any) {
    const ring = feature.getGeometry().getCoordinates()[0]
      .map((point: number[]) => this.ol.toLonLat(point))
      .map(([longitude, latitude]: number[]) => [Number(longitude.toFixed(6)), Number(latitude.toFixed(6))]);
    const label = this.activeDrawLabel;

    this.polygons[label] = {
      label,
      coordinates: this.closeRing(ring),
      area: this.calculateArea(ring)
    };

    this.activeDrawLabel = this.nextDrawLabel(label);
    this.result = null;
    this.renderAll(true);
    this.refresh();

    if (this.hasAllInputs()) {
      void this.saveManualGeometries(true);
    }
  }

  private renderAll(fit = false) {
    if (!this.ol || !this.source) return;
    this.source.clear();
    const renderedCoordinates: number[][] = [];

    for (const label of this.drawLabels) {
      const polygon = this.polygons[label];
      if (!polygon) continue;
      this.addPolygonFeature(label, polygon.coordinates, this.getInputStyle(label));
      renderedCoordinates.push(...polygon.coordinates);
    }

    if (this.result && !this.result.isEmpty) {
      if (this.result.savedLabel) {
        const parsed = this.parseGeometryCoordinates(this.result.geometryJson);
        parsed.forEach(ring => {
          this.addPolygonFeature(this.result!.savedLabel!, ring, {
            fill: 'rgba(18, 148, 91, 0.20)',
            stroke: '#12945b',
            width: 4
          });
          renderedCoordinates.push(...ring);
        });
      } else {
        this.addPolygonFeature('∩' as GeometryLabel, this.result.coordinates, {
          fill: 'rgba(182, 95, 18, 0.26)',
          stroke: '#b65f12',
          width: 4
        });
        renderedCoordinates.push(...this.result.coordinates);
      }
    }

    if (fit && renderedCoordinates.length > 0) {
      const projected = renderedCoordinates.map(point => this.ol.fromLonLat(point));
      this.map.getView().fit(this.ol.boundingExtent(projected), { padding: [44, 44, 44, 44], maxZoom: 16, duration: 220 });
    }
  }

  private addPolygonFeature(label: GeometryLabel, coordinates: number[][], style: { fill: string; stroke: string; width: number }) {
    const projectedRing = this.closeRing(coordinates).map(point => this.ol.fromLonLat(point));
    const feature = new this.ol.Feature({
      geometry: new this.ol.Polygon([projectedRing])
    });
    feature.setStyle(new this.ol.Style({
      fill: new this.ol.Fill({ color: style.fill }),
      stroke: new this.ol.Stroke({ color: style.stroke, width: style.width }),
      text: new this.ol.Text({
        text: label,
        fill: new this.ol.Fill({ color: '#172033' }),
        stroke: new this.ol.Stroke({ color: '#ffffff', width: 4 }),
        font: '700 16px Arial'
      })
    }));
    this.source.addFeature(feature);
  }

  private getInputStyle(label: GeometryLabel): { fill: string; stroke: string; width: number } {
    const styles: Record<string, { fill: string; stroke: string; width: number }> = {
      1: { fill: 'rgba(47, 111, 237, 0.16)', stroke: '#2f6fed', width: 3 },
      2: { fill: 'rgba(198, 122, 28, 0.17)', stroke: '#c67a1c', width: 3 },
      3: { fill: 'rgba(75, 139, 92, 0.16)', stroke: '#4b8b5c', width: 3 }
    };
    return styles[label] ?? { fill: 'rgba(23, 32, 51, 0.12)', stroke: '#172033', width: 3 };
  }

  private nextDrawLabel(current: InputLabel): InputLabel {
    if (current === '1') return '2';
    if (current === '2') return '3';
    return '3';
  }

  polygonOptionLabel(polygon: StoredPolygon): string {
    return `#${polygon.id} ${polygon.label} (User ${polygon.ownerId})`;
  }

  private getRunId(): string {
    return `${Date.now().toString(36)}-${this.getRunIdSuffix()}`;
  }

  private getRunIdSuffix(): string {
    if (globalThis.crypto?.randomUUID) {
      return globalThis.crypto.randomUUID().slice(0, 8);
    }

    if (globalThis.crypto?.getRandomValues) {
      const values = new Uint8Array(4);
      globalThis.crypto.getRandomValues(values);
      return Array.from(values, value => value.toString(16).padStart(2, '0')).join('');
    }

    this.runIdSequence += 1;
    return this.runIdSequence.toString(36).padStart(8, '0');
  }

  private parsePolygonCoordinates(value: string): number[][] {
    return this.parseGeometryCoordinates(value)[0] ?? [];
  }

  private parseGeometryCoordinates(value: string): number[][][] {
    try {
      const parsed = JSON.parse(value);
      const rings = this.extractRings(parsed);
      if (rings.length > 0) return rings;
    } catch {
      const ring = this.readCoordinatePairs(value);
      return ring.length >= 4 ? [this.closeRing(ring)] : [];
    }

    const ring = this.readCoordinatePairs(value);
    return ring.length >= 4 ? [this.closeRing(ring)] : [];
  }

  private extractRings(value: unknown): number[][][] {
    if (!value) return [];

    if (typeof value === 'object' && !Array.isArray(value)) {
      const geometry = value as { type?: string; coordinates?: unknown; geometry?: unknown };
      if (geometry.type === 'Feature') return this.extractRings(geometry.geometry);
      if (geometry.type === 'Polygon') return this.extractCoordinateArray(geometry.coordinates);
      if (geometry.type === 'MultiPolygon') return this.extractCoordinateArray(geometry.coordinates);
      if (geometry.coordinates) return this.extractCoordinateArray(geometry.coordinates);
    }

    return this.extractCoordinateArray(value);
  }

  private extractCoordinateArray(value: unknown): number[][][] {
    if (this.isPointRing(value)) {
      const ring = this.normalizeRing(value);
      return ring.length >= 4 ? [this.closeRing(ring)] : [];
    }

    if (!Array.isArray(value)) return [];

    return value.flatMap(item => this.extractCoordinateArray(item));
  }

  private isPointRing(value: unknown): value is unknown[] {
    return Array.isArray(value) &&
      value.length >= 4 &&
      value.every(point =>
        Array.isArray(point) &&
        point.length >= 2 &&
        Number.isFinite(Number(point[0])) &&
        Number.isFinite(Number(point[1])));
  }

  private readCoordinatePairs(value: string): number[][] {
    const matches = value.match(/-?\d+(?:[.,]\d+)?/g) ?? [];
    const numbers = matches
      .map(match => Number(match.replace(',', '.')))
      .filter(number => Number.isFinite(number));
    const ring: number[][] = [];

    for (let i = 0; i + 1 < numbers.length; i += 2) {
      ring.push([numbers[i], numbers[i + 1]]);
    }

    return ring;
  }

  private normalizeRing(ring: unknown): number[][] {
    if (!Array.isArray(ring)) return [];
    return ring
      .map(point => Array.isArray(point) ? [Number(point[0]), Number(point[1])] : null)
      .filter((point): point is number[] => !!point && Number.isFinite(point[0]) && Number.isFinite(point[1]));
  }

  private toPolygonJson(coordinates: number[][]): string {
    return JSON.stringify({ type: 'Polygon', coordinates: [this.closeRing(coordinates)] });
  }

  private toMultiPolygonJson(rings: number[][][]): string {
    return JSON.stringify({ type: 'MultiPolygon', coordinates: rings.map(ring => [this.closeRing(ring)]) });
  }

  private closeRing(points: number[][]): number[][] {
    if (points.length === 0) return [];
    const ring = points.map(point => [Number(point[0]), Number(point[1])]);
    const first = ring[0];
    const last = ring.at(-1)!;
    return first[0] === last[0] && first[1] === last[1] ? ring : [...ring, first];
  }

  private calculateArea(points: number[][]): number {
    const ring = this.closeRing(points);
    if (ring.length < 4) return 0;
    const averageLatitude = ring.reduce((sum, point) => sum + point[1], 0) / ring.length * Math.PI / 180;
    const earthRadius = 6378137;
    let area = 0;

    for (let i = 0; i < ring.length - 1; i++) {
      const current = this.project(ring[i], averageLatitude, earthRadius);
      const next = this.project(ring[i + 1], averageLatitude, earthRadius);
      area += current.x * next.y - next.x * current.y;
    }

    return Math.round(Math.abs(area) / 2);
  }

  private calculateUnionArea(polygons: number[][][]): number {
    const base = polygons.reduce((sum, polygon) => sum + this.calculateArea(polygon), 0);
    if (polygons.length === 2) {
      return Math.max(0, base - this.calculateArea(this.intersectPolygons(polygons[0], polygons[1])));
    }

    const ab = this.calculateArea(this.intersectPolygons(polygons[0], polygons[1]));
    const ac = this.calculateArea(this.intersectPolygons(polygons[0], polygons[2]));
    const bc = this.calculateArea(this.intersectPolygons(polygons[1], polygons[2]));
    return Math.max(0, base - ab - ac - bc);
  }

  private project(point: number[], averageLatitude: number, earthRadius: number): { x: number; y: number } {
    return {
      x: earthRadius * point[0] * Math.PI / 180 * Math.cos(averageLatitude),
      y: earthRadius * point[1] * Math.PI / 180
    };
  }

  private intersectPolygons(subject: number[][], clip: number[][]): number[][] {
    let output = this.closeRing(subject).slice(0, -1);
    const clipRing = this.ensureCounterClockwise(this.closeRing(clip).slice(0, -1));

    for (let i = 0; i < clipRing.length; i++) {
      const edgeStart = clipRing[i];
      const edgeEnd = clipRing[(i + 1) % clipRing.length];
      const input = output;
      output = [];
      if (input.length === 0) break;

      let previous = input.at(-1)!;
      for (const current of input) {
        if (this.isInside(current, edgeStart, edgeEnd)) {
          if (!this.isInside(previous, edgeStart, edgeEnd)) {
            output.push(this.lineIntersection(previous, current, edgeStart, edgeEnd));
          }
          output.push(current);
        } else if (this.isInside(previous, edgeStart, edgeEnd)) {
          output.push(this.lineIntersection(previous, current, edgeStart, edgeEnd));
        }
        previous = current;
      }
    }

    return this.closeRing(output);
  }

  private ensureCounterClockwise(points: number[][]): number[][] {
    return this.signedArea(points) < 0 ? [...points].reverse() : points;
  }

  private signedArea(points: number[][]): number {
    let area = 0;
    for (let i = 0; i < points.length; i++) {
      const current = points[i];
      const next = points[(i + 1) % points.length];
      area += current[0] * next[1] - next[0] * current[1];
    }
    return area / 2;
  }

  private isInside(point: number[], edgeStart: number[], edgeEnd: number[]): boolean {
    return ((edgeEnd[0] - edgeStart[0]) * (point[1] - edgeStart[1]) -
      (edgeEnd[1] - edgeStart[1]) * (point[0] - edgeStart[0])) >= -1e-10;
  }

  private lineIntersection(a: number[], b: number[], c: number[], d: number[]): number[] {
    const denominator = (a[0] - b[0]) * (c[1] - d[1]) - (a[1] - b[1]) * (c[0] - d[0]);
    if (Math.abs(denominator) < 1e-12) return b;

    const x = ((a[0] * b[1] - a[1] * b[0]) * (c[0] - d[0]) - (a[0] - b[0]) * (c[0] * d[1] - c[1] * d[0])) / denominator;
    const y = ((a[0] * b[1] - a[1] * b[0]) * (c[1] - d[1]) - (a[1] - b[1]) * (c[0] * d[1] - c[1] * d[0])) / denominator;
    return [Number(x.toFixed(6)), Number(y.toFixed(6))];
  }

  private refresh() {
    this.cdr.detectChanges();
  }
}
