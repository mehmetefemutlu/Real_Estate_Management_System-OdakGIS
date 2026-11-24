import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormControl, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { NeighbourhoodService } from './neighbourhood-list.service';
import { DistrictService } from '../../district/district-list/district-list.service';
import { NeighbourhoodView, CreateNeighbourhood } from '../../../models/neighbourhood.model';
import { DistrictView } from '../../../models/district.model';

@Component({
  selector: 'app-neighbourhood-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './neighbourhood-list.component.html',
  styleUrl: './neighbourhood-list.component.scss'
})
export class NeighbourhoodListComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);
  neighbourhoods: NeighbourhoodView[] = [];
  districts: DistrictView[] = [];
  loading = false;
  error = '';
  showForm = false;
  saving = false;
  newNeighbourhood: CreateNeighbourhood = { name: '', districtId: 0 };
  neighbourhoodForm = this.fb.group({
    name: ['', Validators.required],
    districtId: [0, Validators.required]
  });
  filterDistrictId = 0;
  filterDistrictControl = new UntypedFormControl(this.filterDistrictId);
  currentPage = 1;
  pageSize = 10;
  pageSizeControl = new UntypedFormControl(this.pageSize);
  pageSizeOptions = [10, 20, 50, 100];

  constructor(
    private readonly neighbourhoodService: NeighbourhoodService,
    private readonly districtService: DistrictService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  async ngOnInit() {
    try {
      this.districts = await firstValueFrom(this.districtService.getAllDistrictsView());
    } catch {
      this.districts = [];
    }
    await this.load();
  }

  async load() {
    this.loading = true;
    this.refresh();
    const obs = this.filterDistrictId
      ? this.neighbourhoodService.getNeighbourhoodsByDistrict(this.filterDistrictId)
      : this.neighbourhoodService.getAllNeighbourhoodsView();
    try {
      this.neighbourhoods = await firstValueFrom(obs);
      this.clampPage();
    } catch {
      this.error = 'Failed to load neighbourhoods.';
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  openForm() {
    this.showForm = true;
    this.newNeighbourhood = { name: '', districtId: 0 };
    this.neighbourhoodForm.reset(this.newNeighbourhood);
    this.refresh();
  }
  cancelForm() { this.showForm = false; this.refresh(); }

  async save() {
    const value = this.neighbourhoodForm.getRawValue();
    this.newNeighbourhood = {
      name: value.name,
      districtId: Number(value.districtId) || 0
    };
    if (!this.newNeighbourhood.name.trim() || !this.newNeighbourhood.districtId) return;
    this.saving = true;
    this.refresh();
    try {
      await firstValueFrom(this.neighbourhoodService.createNeighbourhood(this.newNeighbourhood));
      this.showForm = false;
      await this.load();
    } catch (err: any) {
      this.error = err?.error?.message || 'Failed to create neighbourhood.';
    } finally {
      this.saving = false;
      this.refresh();
    }
  }

  async delete(id: number, name: string) {
    if (!confirm(`Delete neighbourhood "${name}"?`)) return;
    try {
      await firstValueFrom(this.neighbourhoodService.deleteNeighbourhood(id));
      await this.load();
    } catch (err: any) {
      this.error = err?.error?.message || 'Failed to delete neighbourhood.';
      this.refresh();
    }
  }

  async applyDistrictFilter() {
    this.filterDistrictId = Number(this.filterDistrictControl.value) || 0;
    this.currentPage = 1;
    await this.load();
  }

  get pagedNeighbourhoods(): NeighbourhoodView[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.neighbourhoods.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.neighbourhoods.length / this.pageSize));
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
