import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { PropertyView } from '../../models/property.model';
import { AuthService } from '../auth/auth.service';
import { PropertyService } from '../property/property-list/property-list.service';
import { PropertyOverviewMapComponent } from '../shared/property-overview-map/property-overview-map.component';

interface DashboardCard {
  title: string;
  description: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, PropertyOverviewMapComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  properties: PropertyView[] = [];
  mapLoading = false;
  mapError = '';

  cards: DashboardCard[] = [
    { title: 'Properties', description: 'Browse and manage real estate listings', route: '/properties', icon: '🏠' },
    { title: 'Cities', description: 'Manage city records', route: '/cities', icon: '🏙️' },
    { title: 'Districts', description: 'Manage district records', route: '/districts', icon: '🗺️' },
    { title: 'Neighbourhoods', description: 'Manage neighbourhood records', route: '/neighbourhoods', icon: '🏘️' },
    { title: 'Users', description: 'Manage user accounts', route: '/users', icon: '👤' },
    { title: 'Logs', description: 'View system activity logs', route: '/logs', icon: '📋' },
  ];

  constructor(
    private readonly propertyService: PropertyService,
    private readonly auth: AuthService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  async ngOnInit() {
    await this.loadMapProperties();
  }

  private async loadMapProperties() {
    this.mapLoading = true;
    this.mapError = '';
    this.refresh();

    try {
      const userId = this.auth.currentUserId;
      this.properties = await this.getMapProperties(userId);
    } catch (err: any) {
      this.properties = [];
      this.mapError = err?.error?.message || err?.message || 'Failed to load map properties.';
    } finally {
      this.mapLoading = false;
      this.refresh();
    }
  }

  private async getMapProperties(userId: number | null): Promise<PropertyView[]> {
    if (this.auth.isAdmin) {
      return firstValueFrom(this.propertyService.getAllPropertiesView());
    }

    if (userId) {
      return firstValueFrom(this.propertyService.getPropertiesByUser(userId));
    }

    return [];
  }

  private refresh() {
    this.cdr.detectChanges();
  }
}
