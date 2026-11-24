import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environment';
import { City, CityView, CreateCity, UpdateCity } from '../../../models/city.model';

@Injectable({ providedIn: 'root' })
export class CityService {
  private readonly baseUrl = `${environment.apiUrl}/City`;

  constructor(private readonly http: HttpClient) {}

  getAllCities(): Observable<City[]> {
    return this.http.get<City[]>(this.baseUrl);
  }

  getAllCitiesView(): Observable<CityView[]> {
    return this.http.get<CityView[]>(`${this.baseUrl}/view`);
  }

  getCityById(id: number): Observable<City> {
    return this.http.get<City>(`${this.baseUrl}/${id}`);
  }

  createCity(dto: CreateCity): Observable<City> {
    return this.http.post<City>(this.baseUrl, dto);
  }

  updateCity(id: number, dto: UpdateCity): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto);
  }

  deleteCity(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  cityExists(id: number): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(`${this.baseUrl}/exists/${id}`);
  }

  cityExistsByName(cityName: string): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(`${this.baseUrl}/exists/name/${cityName}`);
  }
}
