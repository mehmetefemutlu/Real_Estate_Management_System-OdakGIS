export interface StoredPolygon {
  id: number;
  ownerId: number;
  label: string;
  coordinates: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateStoredPolygon {
  label: string;
  coordinates: string;
}

export interface UpsertStoredPolygons {
  polygons: CreateStoredPolygon[];
}

export interface CreateStoredPolygons {
  polygons: CreateStoredPolygon[];
}
