import { emptyString } from './Constants';
import { defaultRequestTimeout } from './Constants';
import { injectable } from "inversify";
import "reflect-metadata";

@injectable()
class ClientSettings {
    public mainApplicationEntryPoint: string = emptyString;
    public mainApplicationErrorPage: string = emptyString;
    public mainApplicationForcedLogoutPage = emptyString;
    
    public settingsUrl: string = emptyString;

    public requestTimeout: number = defaultRequestTimeout;

    public urlGeoJsonMap = "./maps/PLsmallgeojson2022-1.0.json";
    public urlPbfGeoJsonMap = "./maps/PLsmallgeojson2022-1.0.pbf";
    public urlPbdfGeoJsonAcurateMap = "./maps/PLallgeojson2022-1.0.pbf";
    public urlGeoJsonDistrictsMap = "./maps/PL-districtsborders2022-1.0.json"

    public mapCenter = [5.277420, 52.1];
    public mapZoom = 7.5;
    public mapZoomMobile = 6.5;

    public urlTiles = "https://localhost:7158/api/Tiles/tile/{z}/{x}/{y}.pbf";

    public messageTimeout = 2000;

    public standaloneVersion = false;
}

const gloClientSettings = new ClientSettings();

export { ClientSettings, gloClientSettings }