import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { DistrictService } from '../../district/district-list/district-list.service';
import { NeighbourhoodListComponent } from './neighbourhood-list.component';
import { NeighbourhoodService } from './neighbourhood-list.service';

describe('NeighbourhoodListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NeighbourhoodListComponent],
      providers: [
        {
          provide: NeighbourhoodService,
          useValue: {
            getAllNeighbourhoodsView: () => of([]),
            getNeighbourhoodsByDistrict: () => of([]),
            createNeighbourhood: () => of({}),
            deleteNeighbourhood: () => of(undefined)
          }
        },
        { provide: DistrictService, useValue: { getAllDistrictsView: () => of([]) } }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(NeighbourhoodListComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
