import type { IMapService } from "../interfaces/IMapService";

import { emptyString } from '../models/Constants'

import "reflect-metadata";
import { injectable, inject } from "inversify";
import { TYPES } from '../types/TYPES';
import { ClientSettings } from '../models/ClientSettings';

import {GeoJSON, FeatureCollection } from "geojson"

import axios from "axios";

@injectable()
export class MapService implements IMapService {
 
    @inject(TYPES.ClientSettings) 
    private _clientSetting?: ClientSettings

    private _map? : GeoJSON;
    private _mapDistricts? : GeoJSON;
    
    private _nameDictionary = new Map<string, string>();

    getMap(): GeoJSON {
        if (this._map){
            return this._map;
        }
              
        throw new Error("Map not loaded");
    }

    getDistrictsMap(): GeoJSON {
        if (this._mapDistricts){
            return this._mapDistricts;
        }
              
        throw new Error("Map not loaded");
    }

    getPlaceCount(): number {
        this.populateDictionaryWhenEmpty();

        return this._nameDictionary.size;
    }

    private populateDictionaryWhenEmpty() {
        if (this._nameDictionary.size == 0) {
            const featureCollection = this._map as FeatureCollection;
            const collection = featureCollection.features;

            collection.forEach(item => {
                if (item.properties) {
                    const pointer = item.properties["featurePointer"];
                    const name = item.properties["popupContent"];
                    const id = pointer?.split(":")[0];

                    if (pointer && name && id && !this._nameDictionary.has(id)) {
                        this._nameDictionary.set(id, name);
                    }
                }
            });
        }
    }

    getPlaces(): string[] {
        this.populateDictionaryWhenEmpty();

        const placesIterator = this._nameDictionary.values();
        const places: string[] = Array.from(placesIterator).sort();

        return places;
    }

    getGroupedPlaces(maxChars: number): string[] {
        const empty: string[] = [];
        
        return this.getGroupedNotCheckedPlaces(maxChars, empty);
    }

    getGroupedNotCheckedPlaces(maxChars: number, checked: string[]): string[] {
        const places = this.getPlaces().filter(p => !(checked.some(c => c == p)));
        const resultLines = [];

        let currentLine = emptyString;

        places.forEach(element => {
            const newLength = 2 + currentLine.length + element.length; 
            
            if (newLength <= maxChars) {
                if (currentLine == emptyString) {
                    currentLine = element;
                } else {
                    currentLine = currentLine + ', ' + element;
                }
            } else {
                resultLines.push(currentLine);

                currentLine = element;
            }
        });

        if (currentLine != emptyString) {
            resultLines.push(currentLine);
        }

        return resultLines;
    }

    getPlaceName(placeId: string): string {
        if (this._map) {

            if (placeId){
                this.populateDictionaryWhenEmpty();

                const name = this._nameDictionary.get(placeId);

                if (name) {
                    return name;
                }
            }
        }

        return emptyString;
    }

    async getGeoJsonMap(): Promise<void> {
        if (this._clientSetting?.urlGeoJsonMap) {
            const response = await axios.get<GeoJSON>(this._clientSetting?.urlGeoJsonMap, { timeout: this._clientSetting?.requestTimeout });

            this._map = response.data;
        } else {
            throw new Error("Url not set");
        }
    }

    async getGeoJsonMapDistricts(): Promise<void> {
        if (this._clientSetting?.urlGeoJsonDistrictsMap) {
            const response = await axios.get<GeoJSON>(this._clientSetting?.urlGeoJsonDistrictsMap, { timeout: this._clientSetting?.requestTimeout });

            this._mapDistricts = response.data;
        } else {
            throw new Error("Url not set");
        }
    }
   
    async loadMap(): Promise<void> {
        await this.getGeoJsonMap();
        await this.getGeoJsonMapDistricts();
    }
    
}