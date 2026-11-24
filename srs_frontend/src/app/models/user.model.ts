export enum UserRole {
  Admin = 0,
  User = 1,
  Guest = 2
}

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  fullName: string;
  createdAt: string;
  updatedAt: string;
  userRole: UserRole;
  propertyCount: number;
}

export interface UserView {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  userRole: UserRole;
}

export interface RegisterUser {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  userRole: UserRole;
}

export interface LoginUser {
  email: string;
  password: string;
}

export interface UpdateUser {
  firstName?: string;
  lastName?: string;
  email?: string;
  password?: string;
  userRole?: UserRole;
}

export interface ChangePassword {
  currentPassword: string;
  newPassword: string;
}

export interface AuthResponse {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  userRole: UserRole;
  token: string;
}
