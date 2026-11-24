import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { UserService } from '../../user/user-list/user-list.service';
import { RegisterUser, UserRole } from '../../../models/user.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly fb = inject(UntypedFormBuilder);
  registerForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    userRole: [UserRole.User, Validators.required]
  });
  error = '';
  success = '';
  loading = false;

  constructor(
    private readonly userService: UserService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  async onSubmit() {
    if (this.loading || this.registerForm.invalid) return;

    this.error = '';
    this.success = '';
    this.loading = true;
    this.cdr.detectChanges();
    try {
      await firstValueFrom(this.userService.registerUser(this.registerForm.getRawValue() as RegisterUser));
      this.success = 'Registration successful! Redirecting to login...';
      setTimeout(() => this.router.navigate(['/login']), 1500);
    } catch (err: any) {
      this.error = err?.error?.message || err?.message || 'Registration failed.';
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }
}
