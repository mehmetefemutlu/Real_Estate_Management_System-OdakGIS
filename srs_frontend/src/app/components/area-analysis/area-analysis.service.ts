import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { CreateStoredPolygon, CreateStoredPolygons, StoredPolygon, UpsertStoredPolygons } from '../../models/polygon.model';

@Injectable({ providedIn: 'root' })
export class PolygonService {
  private readonly baseUrl = `${environment.apiUrl}/Polygon`;

  constructor(private readonly http: HttpClient) {}

  getPolygons(labels?: string[]): Observable<StoredPolygon[]> {
    let params = new HttpParams();
    if (labels?.length) {
      params = params.set('labels', labels.join(','));
    }
    return this.http.get<StoredPolygon[]>(this.baseUrl, { params });
  }

  upsertPolygons(dto: UpsertStoredPolygons): Observable<StoredPolygon[]> {
    return this.http.post<StoredPolygon[]>(`${this.baseUrl}/upsert`, dto);
  }

  createPolygon(dto: CreateStoredPolygon): Observable<StoredPolygon> {
    return this.http.post<StoredPolygon>(this.baseUrl, dto);
  }

  createPolygons(dto: CreateStoredPolygons): Observable<StoredPolygon[]> {
    return this.http.post<StoredPolygon[]>(`${this.baseUrl}/bulk`, dto);
  }
}
