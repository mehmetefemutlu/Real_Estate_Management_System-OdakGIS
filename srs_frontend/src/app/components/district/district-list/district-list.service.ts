import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environment';
import { District, DistrictView, CreateDistrict, UpdateDistrict } from '../../../models/district.model';

@Injectable({ providedIn: 'root' })
export class DistrictService {
  private readonly baseUrl = `${environment.apiUrl}/District`;

  constructor(private readonly http: HttpClient) {}

  getAllDistricts(): Observable<District[]> {
    return this.http.get<District[]>(this.baseUrl);
  }

  getAllDistrictsView(): Observable<DistrictView[]> {
    return this.http.get<DistrictView[]>(`${this.baseUrl}/view`);
  }

  getDistrictById(id: number): Observable<District> {
    return this.http.get<District>(`${this.baseUrl}/${id}`);
  }

  getDistrictsByCity(cityId: number): Observable<DistrictView[]> {
    return this.http.get<DistrictView[]>(`${this.baseUrl}/city/${cityId}`);
  }

  createDistrict(dto: CreateDistrict): Observable<District> {
    return this.http.post<District>(this.baseUrl, dto);
  }

  updateDistrict(id: number, dto: UpdateDistrict): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto);
  }

  deleteDistrict(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  districtExists(id: number): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(`${this.baseUrl}/exists/${id}`);
  }
}
