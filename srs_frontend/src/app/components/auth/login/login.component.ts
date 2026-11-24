import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { UserService } from '../../user/user-list/user-list.service';
import { LoginUser, UserRole } from '../../../models/user.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly fb = inject(UntypedFormBuilder);
  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });
  error = '';
  loading = false;

  constructor(
    private readonly userService: UserService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  async onSubmit() {
    if (this.loading || this.loginForm.invalid) return;

    this.error = '';
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const res = await firstValueFrom(this.userService.loginUser(this.loginForm.getRawValue() as LoginUser));
      localStorage.removeItem('guest');
      localStorage.setItem('token', res.token);
      localStorage.setItem('user', JSON.stringify(res));
      const targetRoute = res.userRole === UserRole.Admin ? '/properties' : '/my-properties';
      await this.router.navigate([targetRoute]);
    } catch (err: any) {
      this.error = err?.error?.message || err?.message || 'Invalid email or password.';
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }
}
