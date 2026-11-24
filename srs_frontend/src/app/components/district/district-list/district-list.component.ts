import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormControl, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { DistrictService } from './district-list.service';
import { CityService } from '../../city/city-list/city-list.service';
import { DistrictView, CreateDistrict } from '../../../models/district.model';
import { CityView } from '../../../models/city.model';

@Component({
  selector: 'app-district-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './district-list.component.html',
  styleUrl: './district-list.component.scss'
})
export class DistrictListComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);
  districts: DistrictView[] = [];
  cities: CityView[] = [];
  loading = false;
  error = '';
  showForm = false;
  saving = false;
  newDistrict: CreateDistrict = { districtName: '', cityId: 0, postalCode: '' };
  districtForm = this.fb.group({
    districtName: ['', Validators.required],
    cityId: [0, Validators.required],
    postalCode: ['']
  });
  filterCityId = 0;
  filterCityControl = new UntypedFormControl(this.filterCityId);
  currentPage = 1;
  pageSize = 10;
  pageSizeControl = new UntypedFormControl(this.pageSize);
  pageSizeOptions = [10, 20, 50, 100];

  constructor(
    private readonly districtService: DistrictService,
    private readonly cityService: CityService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  async ngOnInit() {
    try {
      this.cities = await firstValueFrom(this.cityService.getAllCitiesView());
    } catch {
      this.cities = [];
    }
    await this.load();
  }

  async load() {
    this.loading = true;
    this.refresh();
    const obs = this.filterCityId
      ? this.districtService.getDistrictsByCity(this.filterCityId)
      : this.districtService.getAllDistrictsView();
    try {
      this.districts = await firstValueFrom(obs);
      this.clampPage();
    } catch {
      this.error = 'Failed to load districts.';
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  openForm() {
    this.showForm = true;
    this.newDistrict = { districtName: '', cityId: 0, postalCode: '' };
    this.districtForm.reset(this.newDistrict);
    this.refresh();
  }
  cancelForm() { this.showForm = false; this.refresh(); }

  async saveDistrict() {
    const value = this.districtForm.getRawValue();
    this.newDistrict = {
      districtName: value.districtName,
      cityId: Number(value.cityId) || 0,
      postalCode: value.postalCode
    };
    if (!this.newDistrict.districtName.trim() || !this.newDistrict.cityId) return;
    this.saving = true;
    this.refresh();
    try {
      await firstValueFrom(this.districtService.createDistrict(this.newDistrict));
      this.showForm = false;
      await this.load();
    } catch (err: any) {
      this.error = err?.error?.message || 'Failed to create district.';
    } finally {
      this.saving = false;
      this.refresh();
    }
  }

  async deleteDistrict(id: number, name: string) {
    if (!confirm(`Delete district "${name}"?`)) return;
    try {
      await firstValueFrom(this.districtService.deleteDistrict(id));
      await this.load();
    } catch (err: any) {
      this.error = err?.error?.message || 'Failed to delete district.';
      this.refresh();
    }
  }

  async applyCityFilter() {
    this.filterCityId = Number(this.filterCityControl.value) || 0;
    this.currentPage = 1;
    await this.load();
  }

  get pagedDistricts(): DistrictView[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.districts.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.districts.length / this.pageSize));
  }

  changePageSize() {
    this.pageSize = Number(this.pageSizeControl.value) || 10;
    this.currentPage = 1;
    this.refresh();
  }

  goToPage(page: number) {
    this.currentPage = Math.min(Math.max(page, 1), this.totalPages);
    this.refresh();
  }

  private clampPage() {
    this.currentPage = Math.min(this.currentPage, this.totalPages);
  }

  private refresh() {
    this.cdr.detectChanges();
  }
}
