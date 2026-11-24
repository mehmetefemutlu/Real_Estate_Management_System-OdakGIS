import { Injectable } from '@angular/core';

export type AppTheme = 'dark' | 'light' | 'blue';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'app-theme';
  private readonly defaultTheme: AppTheme = 'light';

  readonly themes: AppTheme[] = ['dark', 'light', 'blue'];

  constructor() {
    this.applyTheme(this.currentTheme);
  }

  get currentTheme(): AppTheme {
    if (typeof localStorage === 'undefined') return this.defaultTheme;

    const storedTheme = localStorage.getItem(this.storageKey);
    return this.isTheme(storedTheme) ? storedTheme : this.defaultTheme;
  }

  setTheme(theme: AppTheme) {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.storageKey, theme);
    }

    this.applyTheme(theme);
  }

  private applyTheme(theme: AppTheme) {
    if (typeof document === 'undefined') return;
    document.documentElement.dataset['theme'] = theme;
  }

  private isTheme(theme: string | null): theme is AppTheme {
    return theme === 'dark' || theme === 'light' || theme === 'blue';
  }
}
