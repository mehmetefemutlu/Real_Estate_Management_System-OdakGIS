import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { CityService } from '../../city/city-list/city-list.service';
import { UserService } from '../../user/user-list/user-list.service';
import { PropertyListComponent } from './property-list.component';
import { PropertyService } from './property-list.service';

describe('PropertyListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PropertyListComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { isAdmin: true, currentUserId: 1 } },
        {
          provide: PropertyService,
          useValue: {
            getAllPropertiesView: () => of([]),
            getPropertiesByUser: () => of([]),
            searchProperties: () => of([]),
            deleteProperty: () => of(undefined),
            deleteProperties: () => of(undefined),
            importPropertiesExcel: () => of({ message: 'ok', count: 0 }),
            exportPropertiesExcel: () => of(new Blob()),
            exportPropertiesPdf: () => of(new Blob())
          }
        },
        { provide: CityService, useValue: { getAllCitiesView: () => of([]) } },
        { provide: UserService, useValue: { getAllUsersView: () => of([]) } }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(PropertyListComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
