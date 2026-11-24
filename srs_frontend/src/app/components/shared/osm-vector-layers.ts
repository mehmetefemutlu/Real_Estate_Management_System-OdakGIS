const LOCAL_BOUNDARY_ASSETS: BoundaryAsset[] = [
  { coordinateKey: 'cities', strokeColor: '#12945b', lineDash: [10, 7] },
  { coordinateKey: 'ankaraDistricts', strokeColor: '#b65f12', lineDash: [5, 5] }
];
const LOCAL_BOUNDARY_ASSET = '/assets/data/administrative-boundaries.json';

type OverlayLoadResult = {
  boundaryCount: number;
};

type BoundaryCoordinateData = {
  cities?: unknown[];
  ankaraDistricts?: unknown[];
};

type BoundaryAsset = {
  coordinateKey: keyof BoundaryCoordinateData;
  strokeColor: string;
  lineDash: number[];
};

type BoundaryRingSet = {
  asset: BoundaryAsset;
  rings: number[][][];
};

let boundaryRingSetsPromise: Promise<BoundaryRingSet[]> | null = null;

export function shouldLoadOsmOverlays(_map?: any): boolean {
  return true;
}

export async function loadAdministrativeBoundaries(
  ol: any,
  _extent: number[],
  boundarySource: any,
  signal?: AbortSignal
): Promise<OverlayLoadResult> {
  const ringSets = await loadLocalBoundaryRingSets(signal);
  let boundaryCount = 0;

  boundarySource.clear();

  for (const ringSet of ringSets) {
    for (const ring of ringSet.rings) {
      if (ring.length < 2) continue;

      const feature = new ol.Feature({
        geometry: new ol.LineString(ring.map(([longitude, latitude]) => ol.fromLonLat([longitude, latitude]))),
        layerType: 'administrative-boundary'
      });
      feature.setStyle(new ol.Style({
        stroke: new ol.Stroke({
          color: ringSet.asset.strokeColor,
          width: 2.5,
          lineDash: ringSet.asset.lineDash
        })
      }));
      boundarySource.addFeature(feature);
      boundaryCount++;
    }
  }

  return { boundaryCount };
}

async function loadLocalBoundaryRingSets(signal?: AbortSignal): Promise<BoundaryRingSet[]> {
  boundaryRingSetsPromise ??= fetch(LOCAL_BOUNDARY_ASSET, { signal })
    .then(response => {
      if (!response.ok) {
        throw new Error(`Boundary asset request failed with ${response.status}`);
      }
      return response.json();
    })
    .then((data: BoundaryCoordinateData) =>
      LOCAL_BOUNDARY_ASSETS.map(asset => ({
        asset,
        rings: parseBoundaryRings(data[asset.coordinateKey])
      })))
    .catch(error => {
      boundaryRingSetsPromise = null;
      throw error;
    });

  return boundaryRingSetsPromise;
}

function parseBoundaryRings(coordinates: unknown): number[][][] {
  if (!Array.isArray(coordinates)) return [];

  if (isCoordinatePair(coordinates[0])) {
    const ring = parseRing(coordinates);
    return ring.length >= 2 ? [ring] : [];
  }

  return coordinates.flatMap(item => parseBoundaryRings(item));
}

function parseRing(ring: unknown): number[][] {
  if (!Array.isArray(ring)) return [];

  return ring
    .map(point => Array.isArray(point) ? [Number(point[0]), Number(point[1])] : null)
    .filter((point): point is number[] =>
      !!point && Number.isFinite(point[0]) && Number.isFinite(point[1]));
}

function isCoordinatePair(value: unknown): value is number[] {
  return Array.isArray(value) &&
    value.length >= 2 &&
    Number.isFinite(Number(value[0])) &&
    Number.isFinite(Number(value[1]));
}
