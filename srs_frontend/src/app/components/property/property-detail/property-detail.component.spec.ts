import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { PropertyDetailComponent } from './property-detail.component';
import { PropertyService } from '../property-list/property-list.service';

describe('PropertyDetailComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PropertyDetailComponent],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => '1' } } } },
        { provide: Router, useValue: { navigate: () => undefined } },
        { provide: AuthService, useValue: { currentUserId: 1 } },
        {
          provide: PropertyService,
          useValue: {
            getPropertyById: () => of({ propertyID: 1, ownerId: 1, isReadOnly: false }),
            deleteProperty: () => of(undefined)
          }
        }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(PropertyDetailComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
