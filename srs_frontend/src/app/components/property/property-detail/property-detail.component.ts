import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { PropertyService } from '../property-list/property-list.service';
import { AuthService } from '../../auth/auth.service';
import { Property, PropertyType } from '../../../models/property.model';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { PropertyMapComponent } from '../../shared/property-map/property-map.component';
import { environment } from '../../../../environment';

@Component({
  selector: 'app-property-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, DecimalPipe, PropertyMapComponent],
  templateUrl: './property-detail.component.html',
  styleUrl: './property-detail.component.scss'
})
export class PropertyDetailComponent implements OnInit {
  property: Property | null = null;
  loading = false;
  error = '';
  PropertyType = PropertyType;

  get canEdit(): boolean {
    if (!this.property) return false;
    return !this.property.isReadOnly && this.property.ownerId === this.auth.currentUserId;
  }

  get propertyImageUrl(): string {
    const path = this.property?.imagePath?.trim();
    if (!path) return '';
    if (/^https?:\/\//i.test(path)) return path;

    const apiRoot = environment.apiUrl.replace(/\/api\/?$/, '');
    const normalized = path.replaceAll('\\', '/');
    if (normalized.startsWith('/uploads/')) return `${apiRoot}${normalized}`;

    const uploadIndex = normalized.toLowerCase().lastIndexOf('/uploads/');
    if (uploadIndex >= 0) {
      const fileName = normalized.split('/').pop();
      return fileName ? `${apiRoot}/uploads/${fileName}` : '';
    }

    return `${apiRoot}/${normalized.replaceAll(/^\/+/g, '')}`;
  }

  get readableCoordinates(): string[] {
    const value = this.property?.coordinates?.trim();
    if (!value) return [];

    try {
      const parsed = JSON.parse(value);
      const coordinates = parsed?.type === 'Polygon' ? parsed.coordinates?.[0] : null;
      if (Array.isArray(coordinates)) {
        return coordinates
          .map((point: unknown, index: number) => this.formatCoordinatePoint(point, index + 1))
          .filter(Boolean) as string[];
      }
    } catch {
      const parts = value.split(/[,\s]+/).map(Number).filter(Number.isFinite);
      if (parts.length >= 2) {
        const [first, second] = parts;
        const latitude = Math.abs(first) <= 90 ? first : second;
        const longitude = Math.abs(first) <= 90 ? second : first;
        return [`Latitude ${latitude.toFixed(6)}, Longitude ${longitude.toFixed(6)}`];
      }
    }

    return [value];
  }

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly propertyService: PropertyService,
    private readonly auth: AuthService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  async ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.router.navigate(['/properties']); return; }
    this.loading = true;
    this.refresh();
    try {
      this.property = await firstValueFrom(this.propertyService.getPropertyById(id));
    } catch {
      this.error = 'Property not found.';
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  getTypeName(type: PropertyType): string {
    return PropertyType[type] ?? 'Unknown';
  }

  private formatCoordinatePoint(point: unknown, index: number): string | null {
    if (!Array.isArray(point)) return null;
    const longitude = Number(point[0]);
    const latitude = Number(point[1]);
    if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) return null;
    return `Point ${index}: Latitude ${latitude.toFixed(6)}, Longitude ${longitude.toFixed(6)}`;
  }

  async delete() {
    if (!this.property || !confirm(`Delete property "${this.property.title}"?`)) return;
    try {
      await firstValueFrom(this.propertyService.deleteProperty(this.property.propertyID));
      sessionStorage.setItem('propertyMessage', 'Property deleted successfully.');
      this.router.navigate(['/my-properties']);
    } catch (err: any) {
      this.error = err?.error?.message || err?.message || 'Delete failed.';
      this.refresh();
    }
  }

  private refresh() {
    this.cdr.detectChanges();
  }
}
