import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environment';
import { Property, PropertyView, CreateProperty, UpdateProperty, SearchProperty } from '../../../models/property.model';

@Injectable({ providedIn: 'root' })
export class PropertyService {
  private readonly baseUrl = `${environment.apiUrl}/Property`;

  constructor(private readonly http: HttpClient) {}

  getAllProperties(): Observable<Property[]> {
    return this.http.get<Property[]>(this.baseUrl);
  }

  getAllPropertiesView(): Observable<PropertyView[]> {
    return this.http.get<PropertyView[]>(`${this.baseUrl}/view`);
  }

  getPropertyById(id: number): Observable<Property> {
    return this.http.get<Property>(`${this.baseUrl}/${id}`);
  }

  getPropertiesByUser(userId: number): Observable<PropertyView[]> {
    return this.http.get<PropertyView[]>(`${this.baseUrl}/user/${userId}`);
  }

  getPropertiesByNeighbourhood(neighbourhoodId: number): Observable<PropertyView[]> {
    return this.http.get<PropertyView[]>(`${this.baseUrl}/neighbourhood/${neighbourhoodId}`);
  }

  getPropertiesByDistrict(districtId: number): Observable<PropertyView[]> {
    return this.http.get<PropertyView[]>(`${this.baseUrl}/district/${districtId}`);
  }

  getPropertiesByCity(cityId: number): Observable<PropertyView[]> {
    return this.http.get<PropertyView[]>(`${this.baseUrl}/city/${cityId}`);
  }

  searchProperties(dto: SearchProperty): Observable<Property[]> {
    return this.http.post<Property[]>(`${this.baseUrl}/search`, dto);
  }

  exportPropertiesExcel(filters: SearchProperty): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/export/excel`, {
      params: this.toParams(filters),
      responseType: 'blob'
    });
  }

  exportPropertiesPdf(filters: SearchProperty): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/export/pdf`, {
      params: this.toParams(filters),
      responseType: 'blob'
    });
  }

  importPropertiesExcel(file: File, userId?: number | null): Observable<{ message: string; count: number }> {
    const formData = new FormData();
    formData.append('file', file);

    let params = new HttpParams();
    if (userId) {
      params = params.set('userId', String(userId));
    }

    return this.http.post<{ message: string; count: number }>(`${this.baseUrl}/import/excel`, formData, { params });
  }

  createProperty(dto: CreateProperty): Observable<Property> {
    return this.http.post<Property>(this.baseUrl, dto);
  }

  updateProperty(id: number, dto: UpdateProperty): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto);
  }

  deleteProperty(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  deleteProperties(propertyIds: number[]): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/bulk-delete`, { propertyIds });
  }

  uploadPropertyImage(id: number, image: File): Observable<{ imagePath: string }> {
    const formData = new FormData();
    formData.append('image', image);
    return this.http.post<{ imagePath: string }>(`${this.baseUrl}/${id}/upload-image`, formData);
  }

  propertyExists(id: number): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(`${this.baseUrl}/exists/${id}`);
  }

  getPropertyCountByUser(userId: number): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.baseUrl}/user/${userId}/count`);
  }

  private toParams(filters: SearchProperty): HttpParams {
    let params = new HttpParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    });
    return params;
  }
}
