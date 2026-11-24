import { PropertyView } from './property.model';

export interface Neighbourhood {
  id: number;
  name: string;
  districtId: number;
  districtName?: string;
  postalCode?: string;
  cityName?: string;
  properties?: PropertyView[];
}

export interface NeighbourhoodView {
  id: number;
  name: string;
  districtName: string;
  cityName: string;
}

export interface CreateNeighbourhood {
  name: string;
  districtId: number;
}

export interface UpdateNeighbourhood {
  name?: string;
  districtId?: number;
}
