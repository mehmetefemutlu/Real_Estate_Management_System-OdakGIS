import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { UserService } from './user-list.service';
import { UserView, UserRole } from '../../../models/user.model';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent implements OnInit {
  users: UserView[] = [];
  loading = false;
  error = '';
  success = '';
  currentPage = 1;
  pageSize = 5;
  pageSizeControl = new UntypedFormControl(this.pageSize);
  UserRole = UserRole;

  constructor(private readonly userService: UserService, private readonly cdr: ChangeDetectorRef) {}

  ngOnInit() {
    const message = sessionStorage.getItem('userMessage');
    if (message) {
      this.success = message;
      sessionStorage.removeItem('userMessage');
    }
    this.load();
  }

  get totalPages(): number {
    return Math.max(Math.ceil(this.users.length / this.pageSize), 1);
  }

  get pagedUsers(): UserView[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.users.slice(start, start + this.pageSize);
  }

  get pageNumbers(): number[] {
    return Array.from({ length: this.totalPages }, (_, index) => index + 1);
  }

  async load() {
    this.loading = true;
    this.error = '';
    this.refresh();
    try {
      this.users = await firstValueFrom(this.userService.getAllUsersView());
      this.goToPage(Math.min(this.currentPage, this.totalPages));
    } catch {
      this.error = 'Failed to load users.';
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  getRoleName(role: UserRole): string {
    return UserRole[role] ?? 'Unknown';
  }

  getRoleBadge(role: UserRole): string {
    switch (role) {
      case UserRole.Admin: return 'badge-blue';
      case UserRole.User: return 'badge-green';
      default: return 'badge-gray';
    }
  }

  async deleteUser(id: number, name: string) {
    if (!confirm(`Want to delete "${name}"? All associated properties will also be deleted.`)) return;
    this.error = '';
    this.success = '';
    try {
      await firstValueFrom(this.userService.deleteUser(id));
      this.success = 'User and associated properties deleted successfully.';
      await this.load();
    } catch (err: any) {
      this.error = err?.error?.message || 'Failed to delete user.';
      this.refresh();
    }
  }

  goToPage(page: number) {
    this.currentPage = Math.min(Math.max(page, 1), this.totalPages);
    this.refresh();
  }

  onPageSizeChange() {
    this.pageSize = Number(this.pageSizeControl.value) || 5;
    this.currentPage = 1;
    this.refresh();
  }

  private refresh() {
    this.cdr.detectChanges();
  }
}
