import { Routes } from '@angular/router';
import { DashboardComponent } from '../components/dashboard/dashboard.component';
import { LoginComponent } from '../components/auth/login/login.component';
import { RegisterComponent } from '../components/auth/register/register.component';
import { CityListComponent } from '../components/city/city-list/city-list.component';
import { DistrictListComponent } from '../components/district/district-list/district-list.component';
import { NeighbourhoodListComponent } from '../components/neighbourhood/neighbourhood-list/neighbourhood-list.component';
import { PropertyListComponent } from '../components/property/property-list/property-list.component';
import { PropertyDetailComponent } from '../components/property/property-detail/property-detail.component';
import { PropertyFormComponent } from '../components/property/property-form/property-form.component';
import { UserListComponent } from '../components/user/user-list/user-list.component';
import { UserFormComponent } from '../components/user/user-form/user-form.component';
import { LogListComponent } from '../components/log/log-list/log-list.component';
import { ProfileComponent } from '../components/profile/profile.component';
import { AreaAnalysisComponent } from '../components/area-analysis/area-analysis.component';
import { authGuard } from '../components/auth/guards/auth.guard';
import { adminGuard } from '../components/auth/guards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard, adminGuard] },
  { path: 'cities', component: CityListComponent, canActivate: [authGuard] },
  { path: 'districts', component: DistrictListComponent, canActivate: [authGuard] },
  { path: 'neighbourhoods', component: NeighbourhoodListComponent, canActivate: [authGuard] },
  { path: 'my-properties', component: PropertyListComponent, canActivate: [authGuard] },
  { path: 'properties', component: PropertyListComponent, canActivate: [authGuard] },
  { path: 'area-analysis', component: AreaAnalysisComponent, canActivate: [authGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [authGuard] },
  { path: 'properties/new', component: PropertyFormComponent, canActivate: [authGuard] },
  { path: 'properties/:id', component: PropertyDetailComponent, canActivate: [authGuard] },
  { path: 'properties/:id/edit', component: PropertyFormComponent, canActivate: [authGuard] },
  { path: 'users', component: UserListComponent, canActivate: [authGuard, adminGuard] },
  { path: 'users/new', component: UserFormComponent, canActivate: [authGuard, adminGuard] },
  { path: 'users/:id/edit', component: UserFormComponent, canActivate: [authGuard, adminGuard] },
  { path: 'logs', component: LogListComponent, canActivate: [authGuard, adminGuard] },
  { path: '**', redirectTo: 'login' }
];
