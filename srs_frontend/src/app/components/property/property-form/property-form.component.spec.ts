import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { CityService } from '../../city/city-list/city-list.service';
import { DistrictService } from '../../district/district-list/district-list.service';
import { NeighbourhoodService } from '../../neighbourhood/neighbourhood-list/neighbourhood-list.service';
import { PropertyFormComponent } from './property-form.component';
import { PropertyService } from '../property-list/property-list.service';

describe('PropertyFormComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PropertyFormComponent],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } } },
        { provide: Router, useValue: { navigate: () => undefined } },
        { provide: AuthService, useValue: { isAdmin: false, currentUserId: 1 } },
        {
          provide: PropertyService,
          useValue: {
            getPropertyById: () => of({}),
            createProperty: () => of({ propertyID: 1 }),
            updateProperty: () => of(undefined),
            uploadPropertyImage: () => of({})
          }
        },
        { provide: CityService, useValue: { getAllCitiesView: () => of([]) } },
        { provide: DistrictService, useValue: { getDistrictsByCity: () => of([]) } },
        { provide: NeighbourhoodService, useValue: { getNeighbourhoodsByDistrict: () => of([]) } }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(PropertyFormComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
