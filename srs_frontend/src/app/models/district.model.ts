import { NeighbourhoodView } from './neighbourhood.model';

export interface District {
  id: number;
  districtName: string;
  cityId: number;
  cityName: string;
  postalCode: string;
  neighbourhoods?: NeighbourhoodView[];
}

export interface DistrictView {
  id: number;
  districtName: string;
  cityName: string;
  postalCode: string;
}

export interface CreateDistrict {
  districtName: string;
  cityId: number;
  postalCode: string;
}

export interface UpdateDistrict {
  districtName?: string;
  cityId?: number;
  postalCode?: string;
}
