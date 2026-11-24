import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { PropertyOverviewMapComponent } from './property-overview-map.component';

describe('PropertyOverviewMapComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PropertyOverviewMapComponent],
      providers: [
        provideRouter([]),
        { provide: Router, useValue: { navigate: () => undefined } }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(PropertyOverviewMapComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
