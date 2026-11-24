import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { LogListComponent } from './log-list.component';
import { LogService } from './log-list.service';

describe('LogListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LogListComponent],
      providers: [
        {
          provide: LogService,
          useValue: {
            getAllLogsView: () => of([]),
            getFilteredLogs: () => of([]),
            deleteLog: () => of(undefined),
            exportLogsExcel: () => of(new Blob()),
            exportLogsPdf: () => of(new Blob())
          }
        }
      ]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(LogListComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
