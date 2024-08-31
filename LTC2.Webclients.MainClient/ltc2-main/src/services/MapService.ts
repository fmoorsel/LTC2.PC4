import type { IMapService } from "../interfaces/IMapService";

import { emptyString } from '../models/Constants'

import "reflect-metadata";
import { injectable, inject } from "inversify";
import { TYPES } from '../types/TYPES';
import { ClientSettings } from '../models/ClientSettings';

import {GeoJSON, FeatureCollection } from "geojson"
import { decode } from "geobuf";
import Pbf from "pbf";

import axios from "axios";

@injectable()
export class MapService implements IMapService {
 
    @inject(TYPES.ClientSettings) 
    private _clientSetting?: ClientSettings

    private _map? : GeoJSON;
    private _mapDistricts? : GeoJSON;
    private _mapFromPbf? : GeoJSON;
    
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

    getMapFromPbf(): GeoJSON {
        if (this._mapFromPbf){
            return this._mapFromPbf;
        }
              
        throw new Error("Map not loaded");
    }

    async getAcurateMapFromPbf(): Promise<GeoJSON> {
        if (this._clientSetting?.urlPbdfGeoJsonAcurateMap) {
            return await this.getPbfGeoJsonFromUrl(this._clientSetting?.urlPbdfGeoJsonAcurateMap);
        }

        throw new Error("Url not set");
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

    async getPbfGeoJsonFromUrl(url: string): Promise<GeoJSON> {
        const responsePbf = await axios.get(url, { 
            timeout: this._clientSetting?.requestTimeout,
            responseType: 'arraybuffer'
        });
        
        const arrBuff = responsePbf.data as ArrayBuffer;
        const byteArray = new Uint8Array(arrBuff);
        const pbf = new Pbf(byteArray);

        return decode(pbf);
    }

    async getPbfGeoJsonMap(): Promise<void> {
        if (this._clientSetting?.urlPbfGeoJsonMap) {
            this._mapFromPbf = await this.getPbfGeoJsonFromUrl(this._clientSetting?.urlPbfGeoJsonMap);
        } else {
            throw new Error("Url not set");
        }
    }
    
    async loadMap(): Promise<void> {
        await this.getGeoJsonMap();
        await this.getGeoJsonMapDistricts();
        await this.getPbfGeoJsonMap();
    }
    
}