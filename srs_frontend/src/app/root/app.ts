import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { NavComponent } from '../components/nav/nav.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, NavComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  currentUrl = '';

  constructor(private readonly router: Router) {
    this.currentUrl = this.router.url;
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(event => {
        this.currentUrl = event.urlAfterRedirects;
      });
  }

  get showNav(): boolean {
    return !this.isAuthRoute(this.currentUrl);
  }

  private isAuthRoute(url: string): boolean {
    return url.startsWith('/login') || url.startsWith('/register');
  }
}
