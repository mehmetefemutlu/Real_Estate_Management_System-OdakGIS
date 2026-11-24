import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormControl } from '@angular/forms';
import { CommonModule, DatePipe, NgClass } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { LogService } from './log-list.service';
import { Log, LogView, OperationType, LogStatus, FilterLog } from '../../../models/log.model';

@Component({
  selector: 'app-log-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgClass, DatePipe],
  templateUrl: './log-list.component.html',
  styleUrl: './log-list.component.scss'
})
export class LogListComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);
  logs: LogView[] = [];
  loading = false;
  error = '';
  OperationType = OperationType;
  LogStatus = LogStatus;
  showFilter = false;
  filterApplied = false;
  pageSizeOptions = [10, 20, 50, 100];
  currentPage = 1;
  pageSize = 10;
  pageSizeControl = new UntypedFormControl(this.pageSize);

  filter: FilterLog = { page: 1, pageSize: 20 };
  filterForm = this.fb.group({
    userId: [null],
    operationType: [undefined],
    status: [undefined],
    description: [''],
    userIP: [''],
    startDate: [''],
    endDate: ['']
  });

  constructor(
    private readonly logService: LogService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit() { this.load(); }

  toggleFilter() {
    this.showFilter = !this.showFilter;
  }

  async load() {
    this.error = '';
    this.loading = true;
    this.refresh();
    try {
      const data = await firstValueFrom(this.logService.getAllLogsView());
      this.logs = data.map(l => this.mapLogView(l));
      this.currentPage = 1;
      this.clampPage();
    } catch {
      this.error = 'Failed to load logs.';
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  async applyFilter(resetPage = true) {
    this.error = '';
    this.filterApplied = true;
    if (resetPage) {
      this.currentPage = 1;
    }
    this.filter = {
      ...this.filterForm.getRawValue(),
      page: this.currentPage,
      pageSize: this.pageSize
    } as FilterLog;
    this.loading = true;
    this.refresh();
    try {
      const data = await firstValueFrom(this.logService.getFilteredLogs(this.filter));
      this.logs = data.map(l => this.mapLog(l));
    } catch {
      this.error = 'Filter failed.';
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  async resetFilter() {
    this.filter = { page: 1, pageSize: this.pageSize };
    this.filterForm.reset({
      userId: null,
      operationType: undefined,
      status: undefined,
      description: '',
      userIP: '',
      startDate: '',
      endDate: ''
    });
    this.filterApplied = false;
    this.currentPage = 1;
    await this.load();
  }

  async goToPage(page: number) {
    if (page < 1 || this.loading) return;

    if (this.filterApplied) {
      if (page > this.currentPage && this.logs.length < this.pageSize) return;
      this.currentPage = page;
      await this.applyFilter(false);
      return;
    }

    this.currentPage = Math.min(Math.max(page, 1), this.totalPages);
    this.refresh();
  }

  get pagedLogs(): LogView[] {
    if (this.filterApplied) return this.logs;
    const start = (this.currentPage - 1) * this.pageSize;
    return this.logs.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.logs.length / this.pageSize));
  }

  get canGoNext(): boolean {
    return this.filterApplied
      ? this.logs.length >= this.pageSize
      : this.currentPage < this.totalPages;
  }

  changePageSize() {
    this.pageSize = Number(this.pageSizeControl.value) || 10;
    this.currentPage = 1;
    this.filter.pageSize = this.pageSize;
    if (this.filterApplied) {
      void this.applyFilter(false);
      return;
    }
    this.clampPage();
    this.refresh();
  }

  getOperationName(op: OperationType): string { return OperationType[op] ?? 'Unknown'; }
  getStatusName(s: LogStatus): string { return LogStatus[s] ?? 'Unknown'; }

  statusBadge(s: LogStatus): string {
    switch (s) {
      case LogStatus.Active: return 'badge-green';
      case LogStatus.Pending: return 'badge-blue';
      case LogStatus.Completed: return 'badge-gray';
      case LogStatus.Aborted: return 'badge-red';
      default: return 'badge-gray';
    }
  }

  async exportCsv() {
    await this.exportFile('excel');
  }

  async exportPdf() {
    await this.exportFile('pdf');
  }

  async deleteLog(id: number) {
    if (!confirm('Delete this log entry?')) return;
    try {
      await firstValueFrom(this.logService.deleteLog(id));
      await this.load();
    } catch (err: any) {
      this.error = err?.error?.message || 'Failed to delete log.';
      this.refresh();
    }
  }

  private refresh() {
    this.cdr.detectChanges();
  }

  private clampPage() {
    this.currentPage = Math.min(Math.max(this.currentPage, 1), this.totalPages);
  }

  private async exportFile(format: 'excel' | 'pdf') {
    this.error = '';
    const exportFilter = this.filterApplied
      ? { ...this.filter }
      : { page: 1, pageSize: 20 };

    try {
      const blob = format === 'excel'
        ? await firstValueFrom(this.logService.exportLogsExcel(exportFilter))
        : await firstValueFrom(this.logService.exportLogsPdf(exportFilter));
      this.downloadBlob(blob, format === 'excel' ? 'system-logs.xlsx' : 'system-logs.pdf');
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

  private mapLog(log: Log): LogView {
    return {
      id: log.id,
      name: log.userName,
      userId: log.userId,
      operationType: log.operationType,
      message: log.message,
      description: log.description,
      status: log.status,
      userIP: log.userIP,
      timestamp: log.createdAt
    };
  }

  private mapLogView(log: LogView): LogView {
    return {
      ...log,
      name: log.name ?? (log as any).userName ?? '',
      timestamp: log.timestamp ?? (log as any).createdAt ?? ''
    };
  }
}
