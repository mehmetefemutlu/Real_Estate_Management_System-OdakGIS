import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserRole, AuthResponse } from '../../../models/user.model';

export const adminGuard: CanActivateFn = () => {
  const router = inject(Router);
  if (typeof localStorage === 'undefined') {
    router.navigate(['/login']);
    return false;
  }

  const stored = localStorage.getItem('user');
  if (stored) {
    try {
      const user: AuthResponse = JSON.parse(stored);
      if (user.userRole === UserRole.Admin) return true;
    } catch {
      localStorage.removeItem('user');
    }
  }
  router.navigate(['/']);
  return false;
};
