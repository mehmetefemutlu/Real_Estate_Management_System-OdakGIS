import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { CityListComponent } from './city-list.component';
import { CityService } from './city-list.service';

describe('CityListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CityListComponent],
      providers: [
        {
          provide: CityService,
          useValue: {
            getAllCitiesView: () => of([]),
            createCity: () => of({}),
            deleteCity: () => of(undefined)
          }
        }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(CityListComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
