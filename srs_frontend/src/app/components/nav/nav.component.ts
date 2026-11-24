import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { UserService } from '../user/user-list/user-list.service';

interface AuthUser {
  firstName: string;
  lastName: string;
  userRole: number;
}

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.scss'
})
export class NavComponent {
  menuOpen = false;

  constructor(private readonly router: Router, private readonly userService: UserService) {}

  toggleMenu() {
    this.menuOpen = !this.menuOpen;
  }

  get currentUser(): AuthUser | null {
    if (typeof localStorage === 'undefined') return null;
    const userStr = localStorage.getItem('user');
    if (!userStr) return null;
    try { return JSON.parse(userStr) as AuthUser; } catch { return null; }
  }

  get isAdmin(): boolean {
    return this.currentUser?.userRole === 0; // 0 = Admin
  }

  get isGuest(): boolean {
    if (typeof localStorage === 'undefined') return false;
    return localStorage.getItem('guest') === 'true' && !this.currentUser;
  }

  goHome() {
    if (this.isAdmin) {
      this.router.navigate(['/dashboard']);
      return;
    }

    if (this.currentUser || this.isGuest) {
      this.router.navigate(['/properties']);
    }
  }

  closeMenu() {
    this.menuOpen = false;
  }

  async logout() {
    try {
      if (localStorage.getItem('token')) {
        await firstValueFrom(this.userService.logoutUser());
      }
    } catch {
      // The user should still be signed out locally if the audit request fails.
    }

    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('guest');
    this.router.navigate(['/login']);
  }
}
