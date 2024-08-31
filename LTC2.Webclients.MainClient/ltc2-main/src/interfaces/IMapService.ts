import {GeoJSON } from "geojson"

export interface IMapService {
    getMap(): GeoJSON;
    getDistrictsMap(): GeoJSON;
    getMapFromPbf(): GeoJSON;
    loadMap(): Promise<void>;

    getPlaceName(placeId: string): string;
    getPlaceCount(): number;
}