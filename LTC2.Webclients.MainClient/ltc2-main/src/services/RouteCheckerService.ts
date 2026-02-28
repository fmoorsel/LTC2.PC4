import type { ISettingsService } from '../interfaces/ISettingsService';
import type { IProfileService } from '../interfaces/IProfileService';
import type { IRouteCheckerService } from '../interfaces/IRouteCheckerService';

import "reflect-metadata";
import { injectable, inject } from "inversify";
import { TYPES } from '../types/TYPES';
import { ClientSettings } from '../models/ClientSettings';
import { NotAuthorizedException } from '../exceptions/NotAuthorizedException';
import { LimitsExceededException } from '../exceptions/LimitsExceededException';

import { Routes } from '../models/Routes';
import { GetRoutesResponse } from '../models/GetRoutesResponse';
import { PresentationRoutes } from '../models/PresentationRoutes';

import { Buffer } from "buffer";

import axios, { AxiosError } from "axios";

@injectable()
export class RouteCheckerService implements IRouteCheckerService {
    
    @inject(TYPES.ISettingsService) 
    private _settingsService?: ISettingsService;

    @inject(TYPES.IProfileService) 
    private _profileService?: IProfileService;

    @inject(TYPES.ClientSettings) 
    private _clientSetting?: ClientSettings

    async checkGpx(file: File): Promise<Routes | undefined> {
        const token = await this._profileService?.getToken();
        
        if (token) {
            const settings = await this._settingsService?.getSettings();
            const url = settings?.routeServiceBaseUrl;
            const timeout = 3 * (this._clientSetting?.requestTimeout ?? 5000);
            
            try {
                const formData = new FormData();
                formData.append("file", file);

                const route = await axios.postForm<Routes>(url + '/api/Route/checkgpx', formData, {headers: {'Authorization': `Bearer ${token}`}, timeout: timeout});

                return route.data;
            } catch(error) {
                console.log(error);

                if(axios.isAxiosError(error)){
                    const axiosError = error as AxiosError;

                    if (axiosError.response?.status == 401){
                        throw new NotAuthorizedException("Missing or expired token.");
                    }
                }

                throw error;
            }
        } else {
            throw new NotAuthorizedException("Missing or expired token.");
        }
    }

    async checkGpxFromPath(file: string): Promise<Routes | undefined> {
        const token = await this._profileService?.getToken();
        
        if (token) {
            const settings = await this._settingsService?.getSettings();
            const url = settings?.routeServiceBaseUrl;
            const timeout = 3 * (this._clientSetting?.requestTimeout ?? 5000);
            
            try {
                const encodedData = Buffer.from(file).toString('base64');

                const route = await axios.get<Routes>(url + '/api/Route/checkgpxfrompath?file=' + encodedData, {headers: {'Authorization': `Bearer ${token}`}, timeout: timeout});

                return route.data;
            } catch(error) {
                console.log(error);

                if(axios.isAxiosError(error)){
                    const axiosError = error as AxiosError;

                    if (axiosError.response?.status == 401){
                        throw new NotAuthorizedException("Missing or expired token.");
                    }
                }

                throw error;
            }
        } else {
            throw new NotAuthorizedException("Missing or expired token.");
        }
    }

    async listRoutes(source?: string) : Promise<PresentationRoutes | undefined> {
        const token = await this._profileService?.getToken();
        
        if (token) {
            const settings = await this._settingsService?.getSettings();
            const url = settings?.routeServiceBaseUrl;
            const timeout = 3 * (this._clientSetting?.requestTimeout ?? 5000);

            try {
                const sourceParam = source ? '?source=' + source : '';
                const routes = await axios.get<GetRoutesResponse>(url + '/api/Route/list' + sourceParam, {headers: {'Authorization': `Bearer ${token}`}, timeout: timeout});
    
                if (routes?.data && routes.data.limitsExceeded) {
                    throw new LimitsExceededException("Strava limits exceeded", routes.data);
                }

                return new PresentationRoutes(routes.data);
            } catch(error) {
                console.log(error);

                if(axios.isAxiosError(error)){
                    const axiosError = error as AxiosError;

                    if (axiosError.response?.status == 401){
                        throw new NotAuthorizedException("Missing or expired token.");
                    }
                }

                throw error;
            }
    } else {
            throw new NotAuthorizedException("Missing or expired token.");
        }
    }

    async checkRoute(routeId: string, source?: string): Promise<Routes | undefined> {
        const token = await this._profileService?.getToken();
        
        if (token) {
            const settings = await this._settingsService?.getSettings();
            const url = settings?.routeServiceBaseUrl;
            const timeout = 3 * (this._clientSetting?.requestTimeout ?? 5000);
            
            try {
                const sourceParam = source ? '&source=' + source : '';
                const route = await axios.get<Routes>(url + '/api/Route/checksourceroute?RouteId=' + routeId + sourceParam, {headers: {'Authorization': `Bearer ${token}`}, timeout: timeout});

                if (route?.data?.limitInfo &&  route?.data?.limitInfo.limitsExceeded) {
                    throw new LimitsExceededException("Strava limits exceeded", route.data.limitInfo);
                }

                return route.data;
            } catch(error) {
                console.log(error);

                if(axios.isAxiosError(error)){
                    const axiosError = error as AxiosError;

                    if (axiosError.response?.status == 401){
                        throw new NotAuthorizedException("Missing or expired token.");
                    }
                }

                throw error;
            }
        } else {
            throw new NotAuthorizedException("Missing or expired token.");
        }
    }
}

