import {GeoJSON } from "geojson"
import { DistrictMapping } from '../models/DistrictMapping'

export interface IMapService {
    getMap(): GeoJSON;
    getDistrictsMap(): GeoJSON;
    getDistrictsMapping(): DistrictMapping[];
    loadMap(): Promise<void>;

    getPlaceName(placeId: string): string;
    getIdForPlaceName(name: string): string;
    getPlaceCount(): number;

    getGroupedNotCheckedPlaces(maxChars: number, checked: string[]): string[];
    getGroupedPlaces(maxChars: number): string[];
    getPlaces(): string[];
}