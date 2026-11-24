export interface City {
  id: number;
  cityName: string;
  districts?: District[];
}

export interface CityView {
  id: number;
  cityName: string;
}

export interface CreateCity {
  cityName: string;
}

export interface UpdateCity {
  cityName: string;
}

import { District } from './district.model';
