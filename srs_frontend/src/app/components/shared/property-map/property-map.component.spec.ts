import { TestBed } from '@angular/core/testing';
import { PropertyMapComponent } from './property-map.component';

describe('PropertyMapComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PropertyMapComponent]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(PropertyMapComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
