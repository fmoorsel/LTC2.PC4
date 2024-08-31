import { Ref } from 'vue';
import { Visit } from '../../models/Visit';
import { Track } from '../../models/Track';

import { Map, View, Overlay } from 'ol';
import { Tile } from 'ol/layer';
import { OSM } from 'ol/source';
import { fromLonLat } from 'ol/proj';

import MVT from 'ol/format/MVT';
import VectorTileLayer from 'ol/layer/VectorTile';
import VectorTileSource from 'ol/source/VectorTile';
import VectorSource from 'ol/source/Vector';
import VectorLayer from 'ol/layer/Vector';
import Feature from 'ol/Feature';
import LineString from 'ol/geom/LineString';

import { Style, Fill, Stroke } from 'ol/style';
import { Control, defaults as defaultControls, FullScreen } from 'ol/control.js';
import { ClientSettings } from '../../models/ClientSettings';

import { IsMobile } from '../../utils/Utils';
import { Extent } from 'ol/extent';

export class MapControl extends Control {
    constructor(mapcontrol: HTMLDivElement) {
        if (mapcontrol) {
            const element = mapcontrol
            
            super({element})
        }else{
            super({});
        }
    }
}

export class MapStyleHelper {
    public static LayerStyle = 0;
    public static LayerStyleVisited = 1;
    public static LayerStyleVisitedYear = 2;
    public static LayerStyleLine = 3;
    public static LayerStyleVisitedTrack = 4;
    public static LayerStyleSelectedPlace = 5;
    public static LayerStyleTrackLine = 6;
    public static LayerStyleTimelapseLine = 7;
    public static LayerStyleTimelapseLineLast = 8;

    private _styles : Style[] = [];

    constructor() {
        this.initStyles();
    }

    public getStyle(styleId: number): Style {
        return this._styles[styleId];
    }

    private initStyles() {
        const layerStyle = new Style({
            fill: new Fill({
                color: "rgba(51, 153, 255, 0.2)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });

        const layerStyleVisited = new Style({
            fill: new Fill({
                color: "rgba(255, 153, 51, 0.65)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });

        const layerStyleVisitedYear = new Style({
            fill: new Fill({
                color: "rgba(204, 51, 0, 0.7)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });                

        const layerStyleLine = new Style({
            stroke: new Stroke({
                color: "rgba(40, 47, 252)",
                width: 2
            })
        });

        const layerStyleVisitedTrack = new Style({
            fill: new Fill({
                color: "rgba(204, 51, 0, 0.7)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });

        const layerStyleSelectedPlace = new Style({
            fill: new Fill({
                color: "rgba(0, 150, 255, 0.7)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });

        const layerStyleTrackLine = new Style({
            stroke: new Stroke({
                color: "rgba(40, 47, 252)",
                width: 2
            })
        });    
        
        const layerStyleTimelapseLine = new Style({
            stroke: new Stroke({
                color: "rgba(40, 47, 252)",
                width: 2
            })
        }); 
        
        const layerStyleTimelapseLineLast = new Style({
            stroke: new Stroke({
                color: "rgba(51, 218, 255)",
                width: 2
            })
        }); 
        this._styles[MapStyleHelper.LayerStyle] = layerStyle;
        this._styles[MapStyleHelper.LayerStyleVisited] = layerStyleVisited;
        this._styles[MapStyleHelper.LayerStyleVisitedYear] = layerStyleVisitedYear;
        this._styles[MapStyleHelper.LayerStyleLine] = layerStyleLine;
        this._styles[MapStyleHelper.LayerStyleVisitedTrack] = layerStyleVisitedTrack;
        this._styles[MapStyleHelper.LayerStyleSelectedPlace] = layerStyleSelectedPlace;
        this._styles[MapStyleHelper.LayerStyleTrackLine] = layerStyleTrackLine;
        this._styles[MapStyleHelper.LayerStyleTimelapseLine] = layerStyleTimelapseLine;
        this._styles[MapStyleHelper.LayerStyleTimelapseLineLast] = layerStyleTimelapseLineLast;
    }
}

export class MapHelper {
    private _mapStyleHelper: MapStyleHelper;
    private _map: Map;
    private _overlay: Overlay;

    private _score: Visit[] | undefined;
    private _scoreYear: Visit[] | undefined;
    private _scoreLast: Visit[] | undefined;

    private _placeholder: HTMLElement | undefined;
    private _popupPlaceholder: HTMLElement | undefined;
    private _mapcontrolPlace: HTMLDivElement | undefined;
    private _clientSettings: ClientSettings | undefined;

    private _vectorTileSource : VectorTileSource;
    private _yearLayer: VectorTileLayer;
    private _lastRidePlacesLayer: VectorTileLayer;
    private _lastRideLineLayer: VectorLayer<VectorSource>;

    private _coordinates : number[][];
    private _place: Ref<string>;

    private _showYear: boolean;
    private _showLast: boolean;
    private _showTrack: boolean;
    private _showTimelapse: boolean;

    private _currentTrack: Track | undefined;
    private _currentPlace: string | undefined;

    private _trackLayer: VectorTileLayer | undefined;
    private _lineTrackLayer: VectorLayer<VectorSource> | undefined;

    private _timelapseLayer: VectorTileLayer | undefined;
    private _linesTimelapseLayer: VectorLayer<VectorSource> | undefined;

    private _timelapseRunning = false;
    private _timelapseBreakRequested = false;

    private _timelapseIndex = 0;

    private _timelapseFeatures : Feature[] = [];

    constructor (
        placeholder: HTMLElement | undefined, 
        popupPlaceHolder: HTMLElement | undefined,
        mapcontrolPlace : HTMLDivElement | undefined,
        score: Visit[] | undefined, 
        scoreYear: Visit[] | undefined, 
        scoreLast: Visit[] | undefined,
        coordinates: number[][],
        place: Ref<string>,
        clientSettings: ClientSettings | undefined
    ) {
        this._mapStyleHelper = new MapStyleHelper();

        this._score = score;
        this._scoreYear = scoreYear;
        this._scoreLast = scoreLast;

        this._showYear = false;
        this._showLast = false;
        this._showTrack = false;
        this._showTimelapse = false;

        this._placeholder = placeholder;
        this._popupPlaceholder = popupPlaceHolder;
        this._mapcontrolPlace = mapcontrolPlace;
        this._clientSettings = clientSettings;

        this._vectorTileSource = this.initVectorTileSource();
        this._map = this.initMap();

        this._coordinates = coordinates;
        this._place = place;

        this._overlay = this.initInteraction();

        this._yearLayer = this.initYearLayer();
        this._lastRidePlacesLayer = this.initLastRidePlacesLayer();
        this._lastRideLineLayer = this.initLastRideLineLayer();
    }

    public getStyleHelper(): MapStyleHelper{
        return this._mapStyleHelper;
    }

    public getCurrentTrack(): Track | undefined {
        return this._currentTrack
    }

    public getStyle(styleId: number): Style {
        return this._mapStyleHelper.getStyle(styleId);
    }

    public getMap(): Map{
        return this._map;
    }

    public getVectorTileSource(): VectorTileSource {
        return this._vectorTileSource;
    }

    public getYearLayer(): VectorTileLayer {
        return this._yearLayer;
    }

    public getLastRidePlacesLayer(): VectorTileLayer {
        return this._lastRidePlacesLayer;
    }

    public getLastRideLineLayer(): VectorLayer<VectorSource> {
        return this._lastRideLineLayer;
    }

    public closePopup() {
        this._overlay.setPosition(undefined)
    }

    public showHideYear() {
        this._showYear = !this._showYear;

        this.removeTrackLayers();
        this.removeTimelapseLayers();

        if (this._showYear) {
            this._map.addLayer(this._yearLayer);
        } else {
            this._map.removeLayer(this._yearLayer);
        }

        if (this._showLast && this._showYear) {
            this._map.removeLayer(this._lastRideLineLayer);
            this._map.removeLayer(this._lastRidePlacesLayer);
        }

        this._showLast = false; 
    }

    public showHideLastRide() {
        this._showLast = !this._showLast;

        this.removeTrackLayers();
        this.removeTimelapseLayers();

        if (this._showLast) {
            this._map.addLayer(this._lastRidePlacesLayer);
            this._map.addLayer(this._lastRideLineLayer);
        } else {
            this._map.removeLayer(this._lastRideLineLayer);
            this._map.removeLayer(this._lastRidePlacesLayer);
        }
        
        if (this._showYear && this._showLast) {
            this._map.removeLayer(this._yearLayer);
        }

        this._showYear = false;
    }

    public showHideTrackForSelectedPlace() {
        const isTrackShowed = this._showTrack;
        
        this.removeTrackLayers();
        this.removeTimelapseLayers();

        if (this._showYear) {
            this._map.removeLayer(this._yearLayer);
            this._showYear = false;
        }

        if (this._showLast) {
            this._map.removeLayer(this._lastRideLineLayer);
            this._map.removeLayer(this._lastRidePlacesLayer);
            this._showLast = false;
        }

        if (!isTrackShowed) {
            if (this._currentPlace && this._currentTrack) {
                this.showTrackForSelectedPlace(this._currentPlace, this._currentTrack, false);
            }
        }
    }

    public getShowYear(): boolean {
        return this._showYear;
    }

    public getShowLastRide(): boolean {
        return this._showLast;
    }

    public getShowTimeLapse(): boolean {
        return this._showTimelapse;
    }

    public isTimelapseRunning(): boolean {
        return this._timelapseRunning;
    }

    public getShowTrackForSelectedPlace(): boolean {
        return this._showTrack
    }

    private removeTrackLayers() {
        if (this._showTrack) {
            if (this._lineTrackLayer) {
                this._map.removeLayer(this._lineTrackLayer);
            }

            if (this._trackLayer) {
                this._map.removeLayer(this._trackLayer);
            }

            this._showTrack = false;
        }
    }

    public removeTimelapseLayers() {
        this._timelapseBreakRequested = this._timelapseRunning;
        
        if (this._linesTimelapseLayer) {
            this._map.removeLayer(this._linesTimelapseLayer);
        }

        if (this._timelapseLayer) {
            this._map.removeLayer(this._timelapseLayer);
        }

        this._linesTimelapseLayer = undefined;
        this._timelapseLayer = undefined;

        this._showTimelapse = false;
    }

    public showTrackForSelectedPlace(placeId: string, track: Track, doZoom = true) {
        this.removeTrackLayers();
        this.removeTimelapseLayers();

        this._currentTrack = track;
        this._currentPlace = placeId;

        const currentTrack = this._currentTrack;
        const currentPlace = this._currentPlace;
        const mapStyleHelper = this._mapStyleHelper;

        this._trackLayer = new VectorTileLayer({
            source: this._vectorTileSource,
            style: function (feature) {
                const featurePointer = feature.getProperties()["featurePointer"] as string
                const id = featurePointer.split(":")[0];

                if (currentTrack && currentTrack.places && currentTrack.places.some(s => s === id)){
                    if (currentPlace && currentPlace === id) {
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleSelectedPlace);
                    } else {
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleVisitedTrack);
                    }
                }

                return new Style();
            }
        });

        this._lineTrackLayer = new VectorLayer({
            source : new VectorSource({
                features: [ 
                    new Feature({
                            geometry: new LineString(currentTrack.coordinates).transform('EPSG:4326', 'EPSG:3857'), 
                        }
                    )
                ]
            }),
            style: mapStyleHelper.getStyle(MapStyleHelper.LayerStyleTrackLine)
        });

        this._map.addLayer(this._trackLayer);
        this._map.addLayer(this._lineTrackLayer);
        
        if (doZoom) {
            const trackExtent = this._lineTrackLayer.getSource()?.getFeatures()[0].getGeometry()?.getExtent();

            if (trackExtent) {
                const center = this.getCenter(trackExtent);
    
                this._map.getView().setCenter(center);
                this._map.getView().setZoom(this.getInitialZoom() + 1);
            }    
        }
        
        this._showTrack = true;
    }

    private removeNonTimelapseLayers() {
        this.removeTrackLayers();
    
        if (this._showYear) {
            this._map.removeLayer(this._yearLayer);
            this._showYear = false;
        }

        if (this._showLast) {
            this._map.removeLayer(this._lastRideLineLayer);
            this._map.removeLayer(this._lastRidePlacesLayer);
            this._showLast = false;
        }
    }

    public getTimelapseIndex(): number {
        return this._timelapseIndex;
    }

    private addTrack(map: MapHelper) {        
        const mapStyleHelper = this._mapStyleHelper;
        const features = this._timelapseFeatures;

        if (!this._linesTimelapseLayer) {
            this._linesTimelapseLayer = new VectorLayer({
                source : new VectorSource({
                    features: features
                }),
                style: function (feature) {
                    const index = feature.getProperties()["index"] + 1
                    if (index == map.getTimelapseIndex()) {
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleTimelapseLineLast);
                    } else if (map.getTimelapseIndex() == -1 || index < map.getTimelapseIndex()) {
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleTimelapseLine);
                    } else {
                        return new Style();
                    }
                }
            });
    
            this._map.addLayer(this._linesTimelapseLayer);
        }

        this._linesTimelapseLayer.changed();
    }

    private addPlaces(trackPlaces: string[], places: Set<string>, newPlaces: Set<string>) {
        newPlaces.clear();

        trackPlaces.forEach( (place) => {
            if (!places.has(place)) {
                places.add(place);
                newPlaces.add(place);
            }
        });

        if (!this._timelapseLayer) {
            const mapStyleHelper = this._mapStyleHelper;

            this._timelapseLayer = new VectorTileLayer({
                source: this._vectorTileSource,
                style: function (feature) {
                    const featurePointer = feature.getProperties()["featurePointer"] as string
                    const id = featurePointer.split(":")[0];
    
                    if (newPlaces.has(id)){
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleSelectedPlace);
                    } else if (places.has(id)){
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleVisitedTrack);
                    }
    
                    return new Style();
                }
            }); 
    
            this._map.addLayer(this._timelapseLayer);
        }
                
        this._timelapseLayer.changed();
    }

    private createAllFeaturesForTimelapse(tracks: Track[]){
        const features: Feature[] = [];
        let counter = 0;

        tracks.forEach(track => {
            const feature = new Feature(new LineString(track.coordinates).transform('EPSG:4326', 'EPSG:3857'));

            feature.setProperties( { "index": counter} );

            counter++;

            features.push(feature);
        });

        return features;
    }

    private doTrackForTimelapse(tracks: Track[], places: Set<string>, newPlaces: Set<string>) {
        if (this._timelapseIndex <= tracks.length && !this._timelapseBreakRequested) {            
            if (this._timelapseIndex < tracks.length) {
                this.addPlaces(tracks[this._timelapseIndex].places, places, newPlaces);
                this.addTrack(this);

                this._timelapseIndex++;
    
                setTimeout( () => {                
                    this.doTrackForTimelapse(tracks, places, newPlaces);    
                }, 200);
            } else {
                this._timelapseIndex = -1;

                this.addPlaces([], places, newPlaces);
                this.addTrack(this);

                this._timelapseBreakRequested = false;
                this._timelapseRunning = false;

                setTimeout( () => {                
                    this.performTimelapse(tracks);    
                }, 200);
            }
        } else {
            this._timelapseBreakRequested = false;
            this._timelapseRunning = false;
        }
    }

    public performTimelapse(tracks: Track[] | undefined) {
        if (this._timelapseRunning || !tracks) {
            this._timelapseBreakRequested = true;
        } else {
            this.removeNonTimelapseLayers();
            this.removeTimelapseLayers();
    
            if (this._timelapseFeatures.length != tracks.length) {
                this._timelapseFeatures = this.createAllFeaturesForTimelapse(tracks);

                if (this._linesTimelapseLayer){
                    this._linesTimelapseLayer.getSource()?.clear();
                    this._linesTimelapseLayer.getSource()?.addFeatures(this._timelapseFeatures);
                }
            }

            if (tracks.length > 0) {
                this._timelapseRunning = true;
                this._timelapseIndex = 0;
                this.doTrackForTimelapse(tracks, new Set<string>(), new Set<string>());
            }

            this._showTimelapse = true;
        }
    }

    private getCenter(extent: Extent) {
        return [(extent[0] + extent[2]) / 2, (extent[1] + extent[3]) / 2];
    }

    private initVectorTileSource(): VectorTileSource {
        const source =  new VectorTileSource({
            format: new MVT(),
            url: this._clientSettings?.urlTiles,
            maxZoom: 17,
        });

        return source;
    }

    private getInitialZoom(): number {
        const mapZoom = this._clientSettings?.mapZoom ?? 7.5;
        const mapZoomMobile = this._clientSettings?.mapZoomMobile ?? 6.5;
        
        return IsMobile() ? mapZoomMobile : mapZoom;
    }

    private initMap(): Map {
        const actualMapZoom = this.getInitialZoom();

        const controls : Control[] = [new FullScreen()];

        if (this._mapcontrolPlace) {
            controls.push(new MapControl(this._mapcontrolPlace));
        }
        
        const map = new Map({
            target: this._placeholder,
            controls: defaultControls().extend(controls),
            layers: [
                new Tile({
                    source: new OSM({maxZoom: 17, attributions: "© 2018-2024 LTC2-The MIT License (MIT)-see program folder for licenses | © <a href=\"https://www.openstreetmap.org/copyright\" target=\"_blank\">OpenStreetMap</a> contributors. "})
                })
            ],
            view: new View({
                center: fromLonLat(this._clientSettings?.mapCenter ?? [5.277420, 52.1]),
                zoom: actualMapZoom,
                minZoom: 3,
                maxZoom: 17
            })
        });

        const score = this._score;
        const styleHelper = this._mapStyleHelper;

        const baseLayer = new VectorTileLayer({
            source: this._vectorTileSource,
            style: function (feature) {
                const featurePointer = feature.getProperties()["featurePointer"] as string
                const id = featurePointer.split(":")[0];
                
                if (score && score.some(s => s.id === id)) {
                    return styleHelper.getStyle(MapStyleHelper.LayerStyleVisited);
                }

                return styleHelper.getStyle(MapStyleHelper.LayerStyle);
            }
        });

        map.addLayer(baseLayer);

        return map;
    }

    private initInteraction(): Overlay {
        const map = this._map;

        const overlay = new Overlay({
            element: this._popupPlaceholder,
            autoPan: true
        });

        map.addOverlay(overlay);

        const place = this._place;

        map.on('singleclick', function (event) {
            if (map.hasFeatureAtPixel(event.pixel) === true) {
                const coordinate = event.coordinate;

                const features = map.getFeaturesAtPixel(event.pixel);

                if (features && features.length > 0) {
                    
                    const feature = features[0];
                    const placeName = feature.getProperties()["popupContent"] as string

                    if (placeName){
                        
                        place.value = placeName;

                        overlay.setPosition(coordinate);
                        
                        return;
                    }
                }
            }

            overlay.setPosition(undefined);
        });
            
        map.on('pointermove', function(event){
            const pixel = map.getEventPixel(event.originalEvent);
            const hit = map.hasFeatureAtPixel(pixel);
            
            map.getViewport().style.cursor = hit ? 'pointer' : '';
        });

        return overlay;
    }

    private initYearLayer(): VectorTileLayer {
        const scoreYear = this._scoreYear;
        const mapStyleHelper = this._mapStyleHelper;

        const ylayer = new VectorTileLayer({
            source: this._vectorTileSource,
            style: function (feature) {
                const featurePointer = feature.getProperties()["featurePointer"] as string
                const id = featurePointer.split(":")[0];

                if (scoreYear && scoreYear.some(s => s.id === id)){
                    return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleVisitedYear);
                }

                return new Style();
            }
        });
        
        return ylayer;
    }

    private initLastRidePlacesLayer(): VectorTileLayer {
        const scoreLast = this._scoreLast;
        const mapStyleHelper = this._mapStyleHelper;

        const llayer = new VectorTileLayer({
            source: this._vectorTileSource,
            style: function (feature) {
                const featurePointer = feature.getProperties()["featurePointer"] as string
                const id = featurePointer.split(":")[0];

                if (scoreLast && scoreLast.some(s => s.id === id)){
                    return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleVisitedYear);
                }

                return new Style();
            }
        });
        
        return llayer;
    }

    private initLastRideLineLayer(): VectorLayer<VectorSource> {
        const coordinates = this._coordinates;
        const mapStyleHelper = this._mapStyleHelper;

        const lineLayer = new VectorLayer({
            source : new VectorSource({
                features: [ 
                    new Feature({
                            geometry: new LineString(coordinates).transform('EPSG:4326', 'EPSG:3857'), 
                        }
                    )
                ]
            }),
            style: mapStyleHelper.getStyle(MapStyleHelper.LayerStyleLine)
        });
        
        return lineLayer;
    } 
}