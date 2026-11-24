import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environment';
import { Log, LogView, CreateLog, FilterLog } from '../../../models/log.model';

@Injectable({ providedIn: 'root' })
export class LogService {
  private readonly baseUrl = `${environment.apiUrl}/Log`;

  constructor(private readonly http: HttpClient) {}

  getAllLogs(): Observable<Log[]> {
    return this.http.get<Log[]>(this.baseUrl);
  }

  getAllLogsView(): Observable<LogView[]> {
    return this.http.get<LogView[]>(`${this.baseUrl}/view`);
  }

  getLogById(id: number): Observable<Log> {
    return this.http.get<Log>(`${this.baseUrl}/${id}`);
  }

  getLogsByUser(userId: number): Observable<Log[]> {
    return this.http.get<Log[]>(`${this.baseUrl}/user/${userId}`);
  }

  getLogsByDateRange(startDate: string, endDate: string): Observable<Log[]> {
    const params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);
    return this.http.get<Log[]>(`${this.baseUrl}/daterange`, { params });
  }

  getFilteredLogs(dto: FilterLog): Observable<Log[]> {
    return this.http.post<Log[]>(`${this.baseUrl}/filter`, dto);
  }

  exportLogsExcel(dto: FilterLog): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/export/excel`, dto, { responseType: 'blob' });
  }

  exportLogsPdf(dto: FilterLog): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/export/pdf`, dto, { responseType: 'blob' });
  }

  createLog(dto: CreateLog): Observable<Log> {
    return this.http.post<Log>(this.baseUrl, dto);
  }

  deleteLog(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
