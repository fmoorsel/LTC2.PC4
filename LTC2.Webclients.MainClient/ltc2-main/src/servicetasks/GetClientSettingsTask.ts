import type { ISeriveTask } from "../interfaces/IServiceTask";

import { ClientSettings } from "../models/ClientSettings";
import { gloClientSettings } from "../models/ClientSettings";

import axios from "axios";

export class GetClientSettingsTask implements ISeriveTask {
    async execute(): Promise<void>{
        console.log('GetClientSettingsTask')

        const clientSettings = await axios.get<ClientSettings>("./clientSettings.json", { timeout: gloClientSettings.requestTimeout });

        console.log(clientSettings.data.settingsUrl)

        gloClientSettings.mainApplicationEntryPoint = clientSettings.data.mainApplicationEntryPoint;
        gloClientSettings.mainApplicationErrorPage = clientSettings.data.mainApplicationErrorPage;
        gloClientSettings.mainApplicationForcedLogoutPage = clientSettings.data.mainApplicationForcedLogoutPage;
        gloClientSettings.settingsUrl = clientSettings.data.settingsUrl;
        gloClientSettings.requestTimeout = clientSettings.data.requestTimeout;
        gloClientSettings.urlGeoJsonMap = clientSettings.data.urlGeoJsonMap;
        gloClientSettings.urlPbfGeoJsonMap = clientSettings.data.urlPbfGeoJsonMap;
        gloClientSettings.urlGeoJsonDistrictsMap = clientSettings.data.urlGeoJsonDistrictsMap;
        gloClientSettings.urlPbdfGeoJsonAcurateMap = clientSettings.data.urlPbdfGeoJsonAcurateMap;
        gloClientSettings.mapCenter = clientSettings.data.mapCenter;
        gloClientSettings.mapZoom = clientSettings.data.mapZoom;
        gloClientSettings.messageTimeout = clientSettings.data.messageTimeout;
        gloClientSettings.urlTiles = clientSettings.data.urlTiles;
        gloClientSettings.standaloneVersion = clientSettings.data.standaloneVersion;

    }
}