import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { UserService } from '../user/user-list/user-list.service';
import { ProfileComponent } from './profile.component';
import { ThemeService } from './profile.service';

describe('ProfileComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfileComponent],
      providers: [
        { provide: Router, useValue: { navigate: () => undefined } },
        {
          provide: AuthService,
          useValue: {
            currentUserId: 1,
            updateStoredUserProfile: () => undefined
          }
        },
        {
          provide: ThemeService,
          useValue: {
            themes: ['dark', 'light', 'blue'],
            currentTheme: 'light',
            setTheme: () => undefined
          }
        },
        {
          provide: UserService,
          useValue: {
            getUserById: () => of({ id: 1 }),
            updateUser: () => of(undefined),
            changePassword: () => of(undefined),
            deleteUser: () => of(undefined)
          }
        }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(ProfileComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
