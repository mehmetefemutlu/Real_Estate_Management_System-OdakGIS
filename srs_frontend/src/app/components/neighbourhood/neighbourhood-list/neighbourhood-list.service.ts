import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environment';
import { Neighbourhood, NeighbourhoodView, CreateNeighbourhood, UpdateNeighbourhood } from '../../../models/neighbourhood.model';

@Injectable({ providedIn: 'root' })
export class NeighbourhoodService {
  private readonly baseUrl = `${environment.apiUrl}/Neighbourhood`;

  constructor(private readonly http: HttpClient) {}

  getAllNeighbourhoods(): Observable<Neighbourhood[]> {
    return this.http.get<Neighbourhood[]>(this.baseUrl);
  }

  getAllNeighbourhoodsView(): Observable<NeighbourhoodView[]> {
    return this.http.get<NeighbourhoodView[]>(`${this.baseUrl}/view`);
  }

  getNeighbourhoodById(id: number): Observable<Neighbourhood> {
    return this.http.get<Neighbourhood>(`${this.baseUrl}/${id}`);
  }

  getNeighbourhoodsByDistrict(districtId: number): Observable<NeighbourhoodView[]> {
    return this.http.get<NeighbourhoodView[]>(`${this.baseUrl}/district/${districtId}`);
  }

  createNeighbourhood(dto: CreateNeighbourhood): Observable<Neighbourhood> {
    return this.http.post<Neighbourhood>(this.baseUrl, dto);
  }

  updateNeighbourhood(id: number, dto: UpdateNeighbourhood): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto);
  }

  deleteNeighbourhood(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  neighbourhoodExists(id: number): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(`${this.baseUrl}/exists/${id}`);
  }
}
