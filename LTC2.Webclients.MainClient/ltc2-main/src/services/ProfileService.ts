import type { ISettingsService } from '../interfaces/ISettingsService';
import type { IProfileService } from '../interfaces/IProfileService';
import type { IMapService } from "../interfaces/IMapService";

import { Profile } from '../models/Profile'
import { Visit } from '../models/Visit'
import { Track } from '../models/Track'
import "reflect-metadata";
import { injectable, inject } from "inversify";
import { TYPES } from '../types/TYPES';
import { emptyString } from '../models/Constants';
import { ClientSettings } from '../models/ClientSettings';
import { NotAuthorizedException } from '../exceptions/NotAuthorizedException';

import axios, { AxiosError } from "axios";

@injectable()
export class ProfileService implements IProfileService {
    
    @inject(TYPES.ISettingsService) 
    private _settingsService?: ISettingsService;

    @inject(TYPES.ClientSettings) 
    private _clientSetting?: ClientSettings

    @inject(TYPES.IMapService) 
    private _mapService?: IMapService

    private _token: string = emptyString;
    private _profile: Profile | undefined = undefined;

    async getToken(): Promise<string> {
        if (this._token){
            
            return this._token;
        
        } else{
            const settings = await this._settingsService?.getSettings();
            const url = settings?.profileServiceBaseUrl;
            
            try {
                const response = await axios.get(url + '/api/Profile/token', {withCredentials: true, timeout: this._clientSetting?.requestTimeout})

                const token = response.data;

                this._token = token;

                return token;
            } catch(error) {
                console.log(error);

                if(axios.isAxiosError(error)){
                    const axiosError = error as AxiosError;

                    if (axiosError.response?.status == 401){
                        console.log('Not authorized!')
                    }
                }

                return emptyString;
            }
        }
    }

    getProfile(): Profile | undefined {
        return this._profile;
    }

    getVisits(): Visit[] {
        if (this._profile) {
            return this._profile.placesInAllTimeScore;    
        } else {
            return new Array<Visit>();
        }
    }
 
    async getAlltimeTrack(placeId: string): Promise<Track> {
        if (this._token) {
            const settings = await this._settingsService?.getSettings();
            const url = settings?.profileServiceBaseUrl;

            try {
                const track = await axios.get<Track>(url + '/api/Profile/profile/track/alltime/' + placeId, {headers: {'Authorization': `Bearer ${this._token}`}, timeout: this._clientSetting?.requestTimeout})

                if (track.data) {
                    return track.data;
                }

                throw new Error('No data found in response');
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
 
    async getAlltimeTracks(): Promise<Track[]> {
        if (this._token) {
            const settings = await this._settingsService?.getSettings();
            const url = settings?.profileServiceBaseUrl;

            try {
                const track = await axios.get<Track[]>(url + '/api/Profile/profile/track/alltime', {headers: {'Authorization': `Bearer ${this._token}`}, timeout: 10 * (this._clientSetting?.requestTimeout ?? 5000)})

                if (track.data) {
                    return track.data;
                }

                throw new Error('No data found in response');
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
    async updateEmail(email: string): Promise<void> {
        if (this._token) {
            const settings = await this._settingsService?.getSettings();
            const url = settings?.profileServiceBaseUrl;
            
            try {
                await axios.post<string>(url + '/api/Profile/profile/email', { email },  {headers: {'Authorization': `Bearer ${this._token}`}, timeout: this._clientSetting?.requestTimeout});
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

    async loadProfile() : Promise<Profile>{
        if (this._token){
            const settings = await this._settingsService?.getSettings();
            const url = settings?.profileServiceBaseUrl;
            
            try {
                const response = await axios.get<Profile>(url + '/api/Profile/profile', {headers: {'Authorization': `Bearer ${this._token}`}, timeout: this._clientSetting?.requestTimeout})

                const profile = response.data;

                if (profile) {
                    if (profile.placesInAllTimeScore) {
                        profile.placesInAllTimeScore.forEach(p => {
                            const name = this._mapService?.getPlaceName(p.id);
    
                            if (name) {
                                p.name = name
                            } else {
                                p.name = p.id
                            }
                        });
    
                        profile.placesInAllTimeScore = profile.placesInAllTimeScore.sort((a, b) => { 
                            return a.name.toLowerCase() > b.name.toLowerCase() ? 1 : -1
                        });
                    }

                    if (profile.placesInLastRideScore && profile.placesInLastRideScore.length >  0) {
                        profile.mostRecentVisitDate = profile.placesInLastRideScore[0].date;
                    }
                }

                this._profile = profile;

                return profile;
            } catch(error) {
                console.log(error);

                if(axios.isAxiosError(error)){
                    const axiosError = error as AxiosError;

                    if (axiosError.response?.status == 401){
                        throw new NotAuthorizedException("Missing token.");
                    }
                }

                throw error;
            }

        } else {
            throw new NotAuthorizedException("Missing token.");
        }
    }
}
