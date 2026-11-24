import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormControl } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CommonModule, DecimalPipe } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { PropertyService } from './property-list.service';
import { CityService } from '../../city/city-list/city-list.service';
import { UserService } from '../../user/user-list/user-list.service';
import { AuthService } from '../../auth/auth.service';
import { PropertyView, PropertyType, SearchProperty } from '../../../models/property.model';
import { CityView } from '../../../models/city.model';
import { UserView } from '../../../models/user.model';
import { PropertyOverviewMapComponent } from '../../shared/property-overview-map/property-overview-map.component';
import { getErrorMessage } from '../../../utils/error-message.util';

@Component({
  selector: 'app-property-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, DecimalPipe, PropertyOverviewMapComponent],
  templateUrl: './property-list.component.html',
  styleUrl: './property-list.component.scss'
})
export class PropertyListComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);
  properties: PropertyView[] = [];
  cities: CityView[] = [];
  users: UserView[] = [];
  loading = false;
  importing = false;
  error = '';
  success = '';
  selectedPropertyIds = new Set<number>();
  PropertyType = PropertyType;

  search: SearchProperty = { page: 1, pageSize: 20 };
  searchForm = this.fb.group({
    minPrice: [undefined],
    maxPrice: [undefined],
    minBedrooms: [undefined],
    minArea: [undefined],
    cityId: [undefined],
    propertyType: [undefined],
    parcelNumber: [''],
    lotNumber: [''],
    ownerId: [undefined]
  });
  showSearch = false;
  currentPage = 1;
  pageSize = 10;
  pageSizeControl = new UntypedFormControl(this.pageSize);
  pageSizeOptions = [10, 20, 50, 100];

  get isAdmin(): boolean { return this.auth.isAdmin; }
  get currentUserId(): number | null { return this.auth.currentUserId; }

  constructor(
    private readonly propertyService: PropertyService,
    private readonly cityService: CityService,
    private readonly userService: UserService,
    private readonly auth: AuthService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  async ngOnInit() {
    const message = sessionStorage.getItem('propertyMessage');
    if (message) {
      this.success = message;
      sessionStorage.removeItem('propertyMessage');
    }

    await Promise.all([
      this.loadCities(),
      this.isAdmin ? this.loadUsers() : Promise.resolve()
    ]);
    await this.load();
  }

  toggleSearch() {
    this.showSearch = !this.showSearch;
  }

  async load() {
    this.error = '';
    this.selectedPropertyIds.clear();
    this.loading = true;
    this.refresh();

    try {
      const userId = this.currentUserId;
      const data = await this.getListProperties(userId);
      this.properties = data ?? [];
      this.clampPage();
    } catch (err: any) {
      this.properties = [];
      this.error = getErrorMessage(err, 'Failed to load properties.');
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  async runSearch() {
    this.error = '';
    this.currentPage = 1;
    this.search = {
      ...this.searchForm.getRawValue(),
      page: 1,
      pageSize: this.pageSize
    } as SearchProperty;
    if (!this.isAdmin) {
      // Regular users can only search within their own properties
      this.search.ownerId = this.currentUserId ?? undefined;
    }
    this.loading = true;
    this.refresh();

    try {
      const data = await firstValueFrom(this.propertyService.searchProperties(this.search));
      this.properties = (data ?? []).map(p => ({
        propertyID: p.propertyID, title: p.title, price: p.price,
        address: p.address, area: p.area, parcelNumber: p.parcelNumber,
        lotNumber: p.lotNumber, coordinates: p.coordinates, propertyType: p.propertyType,
        isReadOnly: p.isReadOnly, districtName: p.districtName, cityName: p.cityName, createdAt: p.createdAt
      }));
      this.clampPage();
    } catch (err: any) {
      this.properties = [];
      this.error = getErrorMessage(err, 'Search failed.');
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  async resetSearch() {
    this.search = { page: 1, pageSize: 20 };
    this.searchForm.reset({
      minPrice: undefined,
      maxPrice: undefined,
      minBedrooms: undefined,
      minArea: undefined,
      cityId: undefined,
      propertyType: undefined,
      parcelNumber: '',
      lotNumber: '',
      ownerId: undefined
    });
    this.currentPage = 1;
    await this.load();
  }

  getTypeName(type: PropertyType): string {
    return PropertyType[type] ?? 'Unknown';
  }

  async deleteProperty(id: number, title: string) {
    if (!confirm(`Want to delete "${title}"?`)) return;
    this.error = '';
    try {
      await firstValueFrom(this.propertyService.deleteProperty(id));
      this.success = 'Property deleted successfully.';
      await this.load();
    } catch (err: any) {
      this.error = getErrorMessage(err, 'Delete failed.');
      this.refresh();
    }
  }

  updateSelection(id: number, event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.checked) {
      this.selectProperty(id);
      return;
    }

    this.deselectProperty(id);
  }

  updateAllSelections(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.checked) {
      this.selectAllPagedProperties();
      return;
    }

    this.clearSelectedProperties();
  }

  selectProperty(id: number) {
    this.selectedPropertyIds.add(id);
  }

  deselectProperty(id: number) {
    this.selectedPropertyIds.delete(id);
  }

  selectAllPagedProperties() {
    this.selectedPropertyIds.clear();
    this.pagedProperties.forEach(p => this.selectedPropertyIds.add(p.propertyID));
  }

  clearSelectedProperties() {
    this.selectedPropertyIds.clear();
  }

  isSelected(id: number): boolean {
    return this.selectedPropertyIds.has(id);
  }

  async deleteSelectedProperties() {
    const ids = Array.from(this.selectedPropertyIds);
    if (ids.length === 0) return;
    if (!confirm(`Want to delete ${ids.length} selected properties?`)) return;

    this.error = '';
    try {
      await firstValueFrom(this.propertyService.deleteProperties(ids));
      this.success = `${ids.length} ${ids.length === 1 ? 'property' : 'properties'} deleted successfully.`;
      this.selectedPropertyIds.clear();
      await this.load();
    } catch (err: any) {
      this.error = getErrorMessage(err, 'Delete failed.');
      this.refresh();
    }
  }

  async exportCsv() {
    await this.exportFile('excel');
  }

  async exportPdf() {
    await this.exportFile('pdf');
  }

  async importExcel(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    this.error = '';
    this.success = '';

    if (!file.name.toLowerCase().endsWith('.xlsx')) {
      this.error = 'Please select an .xlsx file.';
      this.refresh();
      return;
    }

    this.importing = true;
    this.refresh();

    try {
      const response = await firstValueFrom(this.propertyService.importPropertiesExcel(file, this.currentUserId));
      const countText = response.count === 1 ? '1 property imported.' : `${response.count} properties imported.`;
      this.success = response.message || countText;
      if (response.message && response.count !== undefined) {
        this.success = `${response.message} ${countText}`;
      }
      await this.load();
    } catch (err: any) {
      this.error = getErrorMessage(err, 'Import failed. Please check the file format and data.');
      this.refresh();
    } finally {
      this.importing = false;
      this.refresh();
    }
  }

  get pagedProperties(): PropertyView[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.properties.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.properties.length / this.pageSize));
  }

  get allPagedSelected(): boolean {
    return this.pagedProperties.length > 0 && this.pagedProperties.every(p => this.selectedPropertyIds.has(p.propertyID));
  }

  changePageSize() {
    this.pageSize = Number(this.pageSizeControl.value) || 10;
    this.currentPage = 1;
    this.selectedPropertyIds.clear();
    this.refresh();
  }

  goToPage(page: number) {
    this.currentPage = Math.min(Math.max(page, 1), this.totalPages);
    this.selectedPropertyIds.clear();
    this.refresh();
  }

  private async loadCities() {
    try {
      this.cities = await firstValueFrom(this.cityService.getAllCitiesView());
    } catch {
      this.cities = [];
    }
    this.refresh();
  }

  private async loadUsers() {
    try {
      this.users = await firstValueFrom(this.userService.getAllUsersView());
    } catch {
      this.users = [];
    }
    this.refresh();
  }

  private async getListProperties(userId: number | null): Promise<PropertyView[]> {
    if (this.isAdmin) {
      return firstValueFrom(this.propertyService.getAllPropertiesView());
    }

    if (userId) {
      return firstValueFrom(this.propertyService.getPropertiesByUser(userId));
    }

    return [];
  }

  private refresh() {
    this.cdr.detectChanges();
  }

  private clampPage() {
    this.currentPage = Math.min(this.currentPage, this.totalPages);
  }

  private async exportFile(format: 'excel' | 'pdf') {
    this.error = '';
    const filters = { ...this.search };
    if (!this.isAdmin) {
      filters.ownerId = this.currentUserId ?? undefined;
    }

    try {
      const blob = format === 'excel'
        ? await firstValueFrom(this.propertyService.exportPropertiesExcel(filters))
        : await firstValueFrom(this.propertyService.exportPropertiesPdf(filters));
      const extension = format === 'excel' ? 'xlsx' : 'pdf';
      const filename = `${this.isAdmin ? 'all-properties' : 'my-properties'}.${extension}`;
      this.downloadBlob(blob, filename);
    } catch {
      this.error = 'Failed to export.';
      this.refresh();
    }
  }

  private downloadBlob(blob: Blob, filename: string) {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
  }
}
