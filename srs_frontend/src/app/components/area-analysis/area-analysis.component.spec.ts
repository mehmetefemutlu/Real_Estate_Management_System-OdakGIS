import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AreaAnalysisComponent } from './area-analysis.component';
import { PolygonService } from './area-analysis.service';

describe('AreaAnalysisComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AreaAnalysisComponent],
      providers: [
        {
          provide: PolygonService,
          useValue: {
            getPolygons: () => of([]),
            createPolygons: () => of([])
          }
        }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(AreaAnalysisComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
