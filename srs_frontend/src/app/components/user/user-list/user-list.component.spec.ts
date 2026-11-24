import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { UserListComponent } from './user-list.component';
import { UserService } from './user-list.service';

describe('UserListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserListComponent],
      providers: [
        provideRouter([]),
        {
          provide: UserService,
          useValue: {
            getAllUsersView: () => of([]),
            deleteUser: () => of(undefined)
          }
        }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(UserListComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
