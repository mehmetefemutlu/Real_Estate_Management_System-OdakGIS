import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { PropertyService } from '../property-list/property-list.service';
import { CityService } from '../../city/city-list/city-list.service';
import { DistrictService } from '../../district/district-list/district-list.service';
import { NeighbourhoodService } from '../../neighbourhood/neighbourhood-list/neighbourhood-list.service';
import { AuthService } from '../../auth/auth.service';
import { CreateProperty, UpdateProperty, PropertyType } from '../../../models/property.model';
import { CityView } from '../../../models/city.model';
import { DistrictView } from '../../../models/district.model';
import { NeighbourhoodView } from '../../../models/neighbourhood.model';
import { PropertyMapComponent } from '../../shared/property-map/property-map.component';
import { getErrorMessage } from '../../../utils/error-message.util';

@Component({
  selector: 'app-property-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, PropertyMapComponent],
  templateUrl: './property-form.component.html',
  styleUrl: './property-form.component.scss'
})
export class PropertyFormComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);
  isEditMode = false;
  propertyId: number | null = null;
  saving = false;
  error = '';

  cities: CityView[] = [];
  districts: DistrictView[] = [];
  neighbourhoods: NeighbourhoodView[] = [];
  selectedCityId = 0;
  selectedDistrictId = 0;
  selectedImage: File | null = null;
  propertyTypes = [
    { label: 'Apartment', value: PropertyType.Apartment },
    { label: 'House', value: PropertyType.House },
    { label: 'Condo', value: PropertyType.Condo },
    { label: 'Townhouse', value: PropertyType.Townhouse },
    { label: 'Land', value: PropertyType.Land },
  ];

  model: CreateProperty = {
    title: '', description: '', price: 0, address: '', isReadOnly: false,
    ownerId: 0, parcelNumber: '', lotNumber: '', area: 0, bedrooms: 0,
    bathrooms: 0, parkingSpaces: 0, propertyType: PropertyType.Apartment,
    neighbourhoodId: 0, coordinates: ''
  };
  propertyForm = this.fb.group({
    title: ['', Validators.required],
    description: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(1)]],
    address: ['', Validators.required],
    isReadOnly: [false],
    ownerId: [0, [Validators.required, Validators.min(1)]],
    parcelNumber: ['', Validators.required],
    lotNumber: ['', Validators.required],
    area: [0, [Validators.required, Validators.min(1)]],
    coordinates: ['', Validators.required],
    bedrooms: [0, [Validators.required, Validators.min(0)]],
    bathrooms: [0, [Validators.required, Validators.min(0)]],
    parkingSpaces: [0, [Validators.required, Validators.min(0)]],
    propertyType: [PropertyType.Apartment, Validators.required],
    cityId: [0, [Validators.required, Validators.min(1)]],
    districtId: [0, [Validators.required, Validators.min(1)]],
    neighbourhoodId: [0, [Validators.required, Validators.min(1)]]
  });

  get isAdmin(): boolean { return this.auth.isAdmin; }

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly propertyService: PropertyService,
    private readonly cityService: CityService,
    private readonly districtService: DistrictService,
    private readonly neighbourhoodService: NeighbourhoodService,
    private readonly auth: AuthService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  async ngOnInit() {
    if (this.isAdmin) {
      this.router.navigate(['/properties']);
      return;
    }

    await this.loadCities();

    const userId = this.auth.currentUserId;
    if (userId) {
      this.model.ownerId = userId;
      this.propertyForm.patchValue({ ownerId: userId });
    }

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.propertyId = Number(id);
      await this.loadPropertyForEdit(this.propertyId);
    }
  }

  onCityChange() {
    this.selectedCityId = Number(this.propertyForm.get('cityId')?.value) || 0;
    this.selectedDistrictId = 0;
    this.model.neighbourhoodId = 0;
    this.districts = [];
    this.neighbourhoods = [];
    this.propertyForm.patchValue({ districtId: 0, neighbourhoodId: 0 });
    if (!this.selectedCityId) return;

    this.loadDistricts(this.selectedCityId);
  }

  onDistrictChange() {
    this.selectedDistrictId = Number(this.propertyForm.get('districtId')?.value) || 0;
    this.model.neighbourhoodId = 0;
    this.neighbourhoods = [];
    this.propertyForm.patchValue({ neighbourhoodId: 0 });
    if (!this.selectedDistrictId) return;

    this.loadNeighbourhoods(this.selectedDistrictId);
  }

  onImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.selectedImage = input.files?.[0] ?? null;
  }

  onAreaChange(area: number) {
    this.model.area = area;
    this.propertyForm.patchValue({ area });
  }

  onCoordinatesChange(coordinates: string | undefined) {
    this.model.coordinates = coordinates ?? '';
    this.propertyForm.patchValue({ coordinates: this.model.coordinates });
  }

  async onSubmit() {
    if (this.saving) return;

    this.error = '';
    this.syncModelFromForm();
    if (this.propertyForm.invalid || !this.isPropertyValid()) {
      this.error = this.isEditMode
        ? 'Please enter valid property details.'
        : 'Please fill in all required fields with valid format.';
      return;
    }

    this.saving = true;
    this.cdr.detectChanges();
    try {
      if (this.isEditMode && this.propertyId) {
        const update: UpdateProperty = { ...this.model };
        await firstValueFrom(this.propertyService.updateProperty(this.propertyId, update));
        await this.handleSuccessfulSave(this.propertyId, 'Property updated successfully.');
      } else {
        const property = await firstValueFrom(this.propertyService.createProperty(this.model));
        await this.handleSuccessfulSave(property.propertyID, 'Property added successfully.');
      }
    } catch (err: any) {
      this.error = getErrorMessage(err, this.isEditMode ? 'Update failed.' : 'Create failed.');
    } finally {
      this.saving = false;
      this.cdr.detectChanges();
    }
  }

  private async handleSuccessfulSave(propertyId: number, message: string) {
    const finish = () => {
      sessionStorage.setItem('propertyMessage', message);
      this.router.navigate(['/my-properties']);
    };

    if (!this.selectedImage) {
      finish();
      return;
    }

    try {
      await firstValueFrom(this.propertyService.uploadPropertyImage(propertyId, this.selectedImage));
    } catch {
      // Property creation succeeded; do not block navigation on a failed optional image upload.
    }
    finish();
  }

  private isPropertyValid(): boolean {
    return !!this.model.title?.trim() &&
      !!this.model.description?.trim() &&
      this.model.price > 0 &&
      this.model.area > 0 &&
      !!this.model.address?.trim() &&
      !!this.model.parcelNumber?.trim() &&
      !!this.model.lotNumber?.trim() &&
      !!this.model.coordinates?.trim() &&
      this.selectedCityId > 0 &&
      this.selectedDistrictId > 0 &&
      this.model.neighbourhoodId > 0 &&
      this.model.ownerId > 0;
  }

  private syncModelFromForm() {
    const value = this.propertyForm.getRawValue();
    this.selectedCityId = Number(value.cityId) || 0;
    this.selectedDistrictId = Number(value.districtId) || 0;
    this.model = {
      title: value.title,
      description: value.description,
      price: Number(value.price) || 0,
      address: value.address,
      isReadOnly: !!value.isReadOnly,
      ownerId: Number(value.ownerId) || 0,
      parcelNumber: value.parcelNumber,
      lotNumber: value.lotNumber,
      area: Number(value.area) || 0,
      coordinates: value.coordinates,
      bedrooms: Number(value.bedrooms) || 0,
      bathrooms: Number(value.bathrooms) || 0,
      parkingSpaces: Number(value.parkingSpaces) || 0,
      propertyType: Number(value.propertyType) as PropertyType,
      neighbourhoodId: Number(value.neighbourhoodId) || 0
    };
  }

  private async loadCities() {
    try {
      this.cities = await firstValueFrom(this.cityService.getAllCitiesView());
    } catch {
      this.cities = [];
    }
    this.cdr.detectChanges();
  }

  private async loadDistricts(cityId: number) {
    try {
      this.districts = await firstValueFrom(this.districtService.getDistrictsByCity(cityId));
    } catch {
      this.districts = [];
    }
    this.cdr.detectChanges();
  }

  private async loadNeighbourhoods(districtId: number) {
    try {
      this.neighbourhoods = await firstValueFrom(this.neighbourhoodService.getNeighbourhoodsByDistrict(districtId));
    } catch {
      this.neighbourhoods = [];
    }
    this.cdr.detectChanges();
  }

  private async loadPropertyForEdit(propertyId: number) {
    try {
      const p = await firstValueFrom(this.propertyService.getPropertyById(propertyId));
      if (p.ownerId !== this.auth.currentUserId) {
        this.router.navigate(['/properties']);
        return;
      }

      this.model = {
        title: p.title, description: p.description, price: p.price,
        address: p.address, isReadOnly: p.isReadOnly, ownerId: p.ownerId,
        parcelNumber: p.parcelNumber, lotNumber: p.lotNumber, area: p.area,
        bedrooms: p.bedrooms, bathrooms: p.bathrooms, parkingSpaces: p.parkingSpaces,
        propertyType: p.propertyType, neighbourhoodId: p.neighbourhoodId,
        coordinates: p.coordinates
      };
      this.propertyForm.reset({
        ...this.model,
        cityId: 0,
        districtId: 0
      });
      await this.prefillLocationSelectors(p.cityName, p.districtName, p.neighbourhoodId);
      this.cdr.detectChanges();
    } catch {
      this.router.navigate(['/properties']);
    }
  }

  private async prefillLocationSelectors(cityName: string, districtName: string, neighbourhoodId: number) {
    const city = this.cities.find(c => c.cityName === cityName);
    if (!city) return;

    this.selectedCityId = city.id;
    this.propertyForm.patchValue({ cityId: city.id });
    await this.loadDistricts(city.id);

    const district = this.districts.find(d => d.districtName === districtName);
    if (!district) return;

    this.selectedDistrictId = district.id;
    this.propertyForm.patchValue({ districtId: district.id });
    await this.loadNeighbourhoods(district.id);
    this.model.neighbourhoodId = neighbourhoodId;
    this.propertyForm.patchValue({ neighbourhoodId });
  }
}
