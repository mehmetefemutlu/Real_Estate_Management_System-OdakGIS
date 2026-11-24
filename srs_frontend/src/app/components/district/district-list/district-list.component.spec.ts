import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { CityService } from '../../city/city-list/city-list.service';
import { DistrictListComponent } from './district-list.component';
import { DistrictService } from './district-list.service';

describe('DistrictListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DistrictListComponent],
      providers: [
        {
          provide: DistrictService,
          useValue: {
            getAllDistrictsView: () => of([]),
            getDistrictsByCity: () => of([]),
            createDistrict: () => of({}),
            deleteDistrict: () => of(undefined)
          }
        },
        { provide: CityService, useValue: { getAllCitiesView: () => of([]) } }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(DistrictListComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
