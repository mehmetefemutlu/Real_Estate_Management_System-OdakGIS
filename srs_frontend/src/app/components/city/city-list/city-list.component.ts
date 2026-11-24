import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormControl, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CityService } from './city-list.service';
import { CityView, CreateCity } from '../../../models/city.model';

@Component({
  selector: 'app-city-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './city-list.component.html',
  styleUrl: './city-list.component.scss'
})
export class CityListComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);
  cities: CityView[] = [];
  loading = false;
  error = '';
  showForm = false;
  newCity: CreateCity = { cityName: '' };
  cityForm = this.fb.group({
    cityName: ['', Validators.required]
  });
  saving = false;
  currentPage = 1;
  pageSize = 10;
  pageSizeControl = new UntypedFormControl(this.pageSize);
  pageSizeOptions = [10, 20, 50, 100];

  constructor(
    private readonly cityService: CityService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit() { this.load(); }

  async load() {
    this.loading = true;
    this.refresh();
    try {
      this.cities = await firstValueFrom(this.cityService.getAllCitiesView());
      this.clampPage();
    } catch {
      this.error = 'Failed to load cities.';
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  openForm() {
    this.showForm = true;
    this.newCity = { cityName: '' };
    this.cityForm.reset(this.newCity);
    this.refresh();
  }
  cancelForm() { this.showForm = false; this.refresh(); }

  async saveCity() {
    this.newCity = this.cityForm.getRawValue() as CreateCity;
    if (!this.newCity.cityName.trim()) return;
    this.saving = true;
    this.refresh();
    try {
      await firstValueFrom(this.cityService.createCity(this.newCity));
      this.showForm = false;
      await this.load();
    } catch {
      this.error = 'Failed to create city.';
    } finally {
      this.saving = false;
      this.refresh();
    }
  }

  async deleteCity(id: number, name: string) {
    if (!confirm(`Delete city "${name}"?`)) return;
    try {
      await firstValueFrom(this.cityService.deleteCity(id));
      await this.load();
    } catch {
      this.error = 'Failed to delete city.';
      this.refresh();
    }
  }

  get pagedCities(): CityView[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.cities.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.cities.length / this.pageSize));
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
