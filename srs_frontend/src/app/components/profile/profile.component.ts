import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { AppTheme, ThemeService } from './profile.service';
import { UserService } from '../user/user-list/user-list.service';
import { ChangePassword, UpdateUser, User, UserRole } from '../../models/user.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);
  loading = false;
  savingProfile = false;
  changingPassword = false;
  deletingAccount = false;

  error = '';
  profileMessage = '';
  passwordMessage = '';
  themeMessage = '';

  currentUser: User | null = null;
  UserRole = UserRole;
  themeOptions: AppTheme[] = [];
  selectedTheme: AppTheme = 'light';
  themeControl = new UntypedFormControl(this.selectedTheme);

  profileModel: UpdateUser = {
    firstName: '',
    lastName: '',
    email: '',
    userRole: UserRole.User
  };

  passwordModel: ChangePassword = {
    currentPassword: '',
    newPassword: ''
  };
  profileForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    userRole: [UserRole.User, Validators.required]
  });
  passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(12)]]
  });

  constructor(
    private readonly auth: AuthService,
    private readonly themeService: ThemeService,
    private readonly userService: UserService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.themeOptions = this.themeService.themes;
    this.selectedTheme = this.themeService.currentTheme;
    this.themeControl.setValue(this.selectedTheme);
  }

  async ngOnInit() {
    await this.loadProfile();
  }

  get currentUserId(): number | null {
    return this.auth.currentUserId;
  }

  get passwordHint(): string {
    return '8-12 characters, with at least one letter, one number, and one special character.';
  }

  getThemeLabel(theme: AppTheme): string {
    return theme.charAt(0).toUpperCase() + theme.slice(1);
  }

  changeTheme(theme: AppTheme) {
    this.selectedTheme = theme;
    this.themeService.setTheme(theme);
    this.themeMessage = 'Theme updated successfully.';
    this.refresh();
  }

  async saveProfile() {
    if (this.savingProfile || !this.currentUserId) return;

    this.error = '';
    this.profileMessage = '';
    this.profileModel = this.profileForm.getRawValue() as UpdateUser;

    if (!this.profileModel.firstName?.trim() || !this.profileModel.lastName?.trim() || !this.profileModel.email?.trim()) {
      this.error = 'Please complete all profile fields.';
      this.refresh();
      return;
    }

    this.savingProfile = true;
    this.refresh();
    try {
      await firstValueFrom(this.userService.updateUser(this.currentUserId, this.profileModel));
      this.auth.updateStoredUserProfile({
        firstName: this.profileModel.firstName.trim(),
        lastName: this.profileModel.lastName.trim(),
        email: this.profileModel.email.trim()
      });
      this.profileMessage = 'Profile updated successfully.';
      await this.loadProfile(false);
    } catch (err: any) {
      this.error = err?.error?.message || err?.message || 'Failed to update profile.';
    } finally {
      this.savingProfile = false;
      this.refresh();
    }
  }

  async changePassword() {
    if (this.changingPassword || !this.currentUserId) return;

    this.error = '';
    this.passwordMessage = '';
    this.passwordModel = this.passwordForm.getRawValue() as ChangePassword;

    if (!this.passwordModel.currentPassword.trim() || !this.passwordModel.newPassword.trim()) {
      this.error = 'Please enter both your current password and a new password.';
      this.refresh();
      return;
    }

    if (!this.isValidPassword(this.passwordModel.newPassword)) {
      this.error = 'Password must be between 8 and 12 characters and include at least one letter, one number, and one special character.';
      this.refresh();
      return;
    }

    this.changingPassword = true;
    this.refresh();
    try {
      await firstValueFrom(this.userService.changePassword(this.currentUserId, this.passwordModel));
      this.passwordModel = { currentPassword: '', newPassword: '' };
      this.passwordForm.reset(this.passwordModel);
      this.passwordMessage = 'Security settings updated successfully.';
    } catch (err: any) {
      this.error = err?.error?.message || err?.message || 'Failed to update password.';
    } finally {
      this.changingPassword = false;
      this.refresh();
    }
  }

  async deleteAccount() {
    if (this.deletingAccount || !this.currentUserId) return;

    const confirmed = globalThis.confirm('This will permanently delete your account and associated records. This action cannot be undone. Continue?');
    if (!confirmed) return;

    this.error = '';
    this.deletingAccount = true;
    this.refresh();

    try {
      await firstValueFrom(this.userService.deleteUser(this.currentUserId));
      if (typeof localStorage !== 'undefined') {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        localStorage.removeItem('guest');
      }
      this.router.navigate(['/login']);
    } catch (err: any) {
      this.error = err?.error?.message || err?.message || 'Failed to delete account.';
    } finally {
      this.deletingAccount = false;
      this.refresh();
    }
  }

  private async loadProfile(showLoader = true) {
    if (!this.currentUserId) return;

    if (showLoader) {
      this.loading = true;
      this.refresh();
    }

    try {
      const user = await firstValueFrom(this.userService.getUserById(this.currentUserId));
      this.currentUser = user;
      this.profileModel = {
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
        userRole: user.userRole
      };
      this.profileForm.reset(this.profileModel);
    } catch (err: any) {
      this.error = err?.error?.message || err?.message || 'Failed to load profile.';
    } finally {
      this.loading = false;
      this.refresh();
    }
  }

  private isValidPassword(password: string): boolean {
    return /^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,12}$/.test(password);
  }

  private refresh() {
    this.cdr.detectChanges();
  }
}
