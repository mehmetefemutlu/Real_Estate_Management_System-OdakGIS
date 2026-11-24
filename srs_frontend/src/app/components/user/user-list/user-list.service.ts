import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environment';
import { User, UserView, RegisterUser, LoginUser, UpdateUser, ChangePassword, AuthResponse } from '../../../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = `${environment.apiUrl}/User`;

  constructor(private readonly http: HttpClient) {}

  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.baseUrl);
  }

  getAllUsersView(): Observable<UserView[]> {
    return this.http.get<UserView[]>(`${this.baseUrl}/view`);
  }

  getUserById(id: number): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/${id}`);
  }

  getUserByEmail(email: string): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/email/${email}`);
  }

  registerUser(dto: RegisterUser): Observable<User> {
    return this.http.post<User>(`${this.baseUrl}/register`, dto);
  }

  loginUser(dto: LoginUser): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, dto);
  }

  logoutUser(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, {});
  }

  updateUser(id: number, dto: UpdateUser): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto);
  }

  changePassword(id: number, dto: ChangePassword): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/password`, dto);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  userExists(id: number): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(`${this.baseUrl}/exists/${id}`);
  }

  emailExists(email: string): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(`${this.baseUrl}/exists/email/${email}`);
  }
}
