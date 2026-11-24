import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { UserFormComponent } from './user-form.component';
import { UserService } from '../user-list/user-list.service';

describe('UserFormComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserFormComponent],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } } },
        { provide: Router, useValue: { navigate: () => undefined } },
        {
          provide: UserService,
          useValue: {
            getUserById: () => of({}),
            updateUser: () => of(undefined),
            registerUser: () => of({})
          }
        }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(UserFormComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
