import { Injectable } from '@angular/core';
import { AuthResponse, UserRole } from '../../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  get currentUser(): AuthResponse | null {
    if (typeof localStorage === 'undefined') return null;
    const stored = localStorage.getItem('user');
    if (!stored) return null;
    try { return JSON.parse(stored) as AuthResponse; } catch { return null; }
  }

  get isAdmin(): boolean {
    return this.currentUser?.userRole === UserRole.Admin;
  }

  get isLoggedIn(): boolean {
    if (typeof localStorage === 'undefined') return false;
    return !!localStorage.getItem('token');
  }

  get currentUserId(): number | null {
    return this.currentUser?.userId ?? null;
  }

  updateStoredUserProfile(profile: { firstName: string; lastName: string; email: string }) {
    if (typeof localStorage === 'undefined') return;
    const current = this.currentUser;
    if (!current) return;

    const updated: AuthResponse = {
      ...current,
      firstName: profile.firstName,
      lastName: profile.lastName,
      email: profile.email
    };

    localStorage.setItem('user', JSON.stringify(updated));
  }
}
