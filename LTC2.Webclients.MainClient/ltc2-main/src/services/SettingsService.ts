import type { ISettingsService } from '../interfaces/ISettingsService';

import { injectable, inject } from "inversify";
import "reflect-metadata";
import { TYPES } from '../types/TYPES';
import { ClientSettings } from '../models/ClientSettings';
import { MainClientSettings } from '../models/MainClientSettings';

import axios from "axios";

@injectable()
export class SettingsService implements ISettingsService {
    
    @inject(TYPES.ClientSettings) 
    private _clientSetting?: ClientSettings

    private _mainClientSettings? : MainClientSettings;

    async getSettings() : Promise<MainClientSettings> {
        if (!this._mainClientSettings) {
            const url = this._clientSetting?.settingsUrl + "/api/Settings/mainclientsettings"

            console.log(url);
    
            const mainClientSettings = await axios.get<MainClientSettings>(url, {timeout: this._clientSetting?.requestTimeout});

            this._mainClientSettings = mainClientSettings.data;
        }
        
        return this._mainClientSettings;
    }
}
 