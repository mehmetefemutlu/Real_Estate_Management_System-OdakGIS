export enum PropertyType {
  Apartment = 0,
  House = 1,
  Condo = 2,
  Townhouse = 3,
  Land = 4
}

export interface Property {
  propertyID: number;
  title: string;
  description: string;
  price: number;
  address: string;
  isReadOnly: boolean;
  ownerId: number;
  ownerName: string;
  ownerEmail: string;
  parcelNumber: string;
  lotNumber: string;
  area: number;
  coordinates: string;
  imagePath: string;
  bedrooms: number;
  bathrooms: number;
  parkingSpaces: number;
  propertyType: PropertyType;
  createdAt: string;
  updatedAt: string;
  neighbourhoodId: number;
  neighbourhoodName: string;
  districtName: string;
  cityName: string;
}

export interface PropertyView {
  propertyID: number;
  title: string;
  price: number;
  address: string;
  area: number;
  coordinates: string;
  parcelNumber: string;
  lotNumber: string;
  propertyType: PropertyType;
  isReadOnly: boolean;
  districtName: string;
  cityName: string;
  createdAt: string;
}

export interface CreateProperty {
  title: string;
  description: string;
  price: number;
  address: string;
  isReadOnly: boolean;
  ownerId: number;
  parcelNumber: string;
  lotNumber: string;
  area: number;
  coordinates?: string;
  bedrooms: number;
  bathrooms: number;
  parkingSpaces: number;
  propertyType: PropertyType;
  neighbourhoodId: number;
}

export interface UpdateProperty {
  title?: string;
  description?: string;
  price?: number;
  address?: string;
  isReadOnly?: boolean;
  parcelNumber?: string;
  lotNumber?: string;
  area?: number;
  coordinates?: string;
  bedrooms?: number;
  bathrooms?: number;
  parkingSpaces?: number;
  propertyType?: PropertyType;
  neighbourhoodId?: number;
}

export interface SearchProperty {
  minPrice?: number;
  maxPrice?: number;
  minBedrooms?: number;
  maxBedrooms?: number;
  minArea?: number;
  maxArea?: number;
  propertyType?: PropertyType;
  cityId?: number;
  districtId?: number;
  neighbourhoodId?: number;
  page?: number;
  pageSize?: number;
  parcelNumber?: string;
  lotNumber?: string;
  ownerId?: number;
}
