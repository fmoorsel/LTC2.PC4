import { Ref } from 'vue';
import { Visit } from '../../models/Visit';
import { Track } from '../../models/Track';
import { Routes } from '../../models/Routes';

import { Map, View, Overlay } from 'ol';
import { Tile } from 'ol/layer';
import { OSM } from 'ol/source';
import { fromLonLat } from 'ol/proj';

import MVT from 'ol/format/MVT';
import GeoJSONFormat from 'ol/format/GeoJSON';
import { GeoJSON } from 'geojson';
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

import { runsInMultiSportMode } from '../../utils/Utils';

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
    public static LayerStyleCheckedPlace = 9;
    public static LayerStyleNewCheckedPlace = 10;
    public static LayerStyleNewYearCheckedPlace = 11;
    public static LayerStyleDistrictsLine = 12;

    private _styles : Style[] = [];

    constructor() {
        this.initStyles();
    }

    public getRawStyle(styleId: number): Style {
        return this._styles[styleId];
    }

    public getStyle(styleId: number, map: Map | undefined): Style {        
        if (map) {
            const zoom = map.getView().getZoom() ?? 0;
            const style = this._styles[styleId];
     
            let multiplier = 1.0;
            
            if (zoom > 14) {
                multiplier =  4.0
            } else if (zoom > 12) {
                multiplier =  3.0
            } else if (zoom > 10) {
                multiplier =  2.0
            } else if (zoom > 8) {
                multiplier =  1.5
            }
     
            const stroke = style.getStroke();
            const fill = style.getFill();
            const widthToUse = multiplier * (stroke?.getWidth() ?? 0.5);
    
            const styletoUse = new Style({
                fill: fill || undefined,
                stroke: new Stroke({
                    color: stroke?.getColor() ?? '#000000',
                    width: widthToUse
                })
            });
    
            return styletoUse;
        }

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
      
        const layerStyleVisitedMulti = new Style({
            fill: new Fill({
                color: "rgba(0, 224, 0, 0.6)",
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

        const layerStyleVisitedYearMulti = new Style({
            fill: new Fill({
                color: "rgba(0, 100, 0, 0.8)",
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

        const layerStyleVisitedTrackMulti = new Style({
            fill: new Fill({
                color: "rgba(0, 100, 0, 0.8)",
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

        const layerStyleCheckedPlace = new Style({
            fill: new Fill({
                color: "rgba(204, 51, 0, 0.7)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });

        const layerStyleCheckedPlaceMulti = new Style({
            fill: new Fill({
                color: "rgba(0, 100, 0, 0.8)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });

        const layerStyleNewCheckedPlace = new Style({
            fill: new Fill({
                color: "rgba(0, 255, 0, 0.7)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });

        const layerStyleNewCheckedPlaceMulti = new Style({
            fill: new Fill({
                color: "rgba(252, 172, 0, 0.7)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });

        const layerStyleNewCheckedYearPlace = new Style({
            fill: new Fill({
                color: "rgba(0, 128, 0, 0.7)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });

        const layerStyleNewCheckedYearPlaceMulti = new Style({
            fill: new Fill({
                color: "rgba(204, 51, 0, 0.7)",
            }),
            stroke: new Stroke({
                color: "rgba(51, 153, 255)",
                width: 0.5
            })
        });

        const layerStyleDistrictBordersLine = new Style({
            stroke: new Stroke({
                color: '#000000',
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
        this._styles[MapStyleHelper.LayerStyleCheckedPlace] = layerStyleCheckedPlace;
        this._styles[MapStyleHelper.LayerStyleNewCheckedPlace] = layerStyleNewCheckedPlace;
        this._styles[MapStyleHelper.LayerStyleNewYearCheckedPlace] = layerStyleNewCheckedYearPlace;
        this._styles[MapStyleHelper.LayerStyleDistrictsLine] = layerStyleDistrictBordersLine;

        if (runsInMultiSportMode()) {
            this._styles[MapStyleHelper.LayerStyleVisited] = layerStyleVisitedMulti;
            this._styles[MapStyleHelper.LayerStyleVisitedYear] = layerStyleVisitedYearMulti;
            this._styles[MapStyleHelper.LayerStyleVisitedTrack] = layerStyleVisitedTrackMulti;
            this._styles[MapStyleHelper.LayerStyleCheckedPlace] = layerStyleCheckedPlaceMulti;
            this._styles[MapStyleHelper.LayerStyleNewCheckedPlace] = layerStyleNewCheckedPlaceMulti;
            this._styles[MapStyleHelper.LayerStyleNewYearCheckedPlace] = layerStyleNewCheckedYearPlaceMulti; 
        } 
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
    private _showRoute: boolean;
    private _showTimelapse: boolean;

    private _currentTrack: Track | undefined;
    private _currentPlace: string | undefined;

    private _currentRoutes: Routes | undefined;

    private _trackLayer: VectorTileLayer | undefined;
    private _lineTrackLayer: VectorLayer<VectorSource> | undefined;
    private _routePlacesLayer: VectorTileLayer | undefined;
    private _routeLineLayer: VectorLayer<VectorSource> | undefined;

    private _timelapseLayer: VectorTileLayer | undefined;
    private _linesTimelapseLayer: VectorLayer<VectorSource> | undefined;

    private _provincesLayer: VectorLayer<VectorSource> | undefined;
    private _showProvinces = false;

    private _todoLayer: VectorTileLayer | undefined;
    private _showTodo = false;

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
        this._showRoute = false;
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

    public getCurrentRoutes(): Routes | undefined {
        return this._currentRoutes
    }

    public getStyle(styleId: number): Style {
        return this._mapStyleHelper.getStyle(styleId, this._map);
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
        this.removeRouteLayers();
        this.removeTodoLayer();

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
        this.removeRouteLayers();
        this.removeTodoLayer();

        if (this._showLast) {
            this._map.addLayer(this._lastRidePlacesLayer);
            this._map.addLayer(this._lastRideLineLayer);
        } else {
            this._map.removeLayer(this._lastRideLineLayer);
            this._map.removeLayer(this._lastRidePlacesLayer);
        }
        
        if (this._showLast && this._showYear) {
            this._map.removeLayer(this._yearLayer);
        }

        this._showYear = false;
    }

    public showHideTrackForSelectedPlace() {
        const isTrackShowed = this._showTrack;

        this.removeTrackLayers();
        this.removeTimelapseLayers();
        this.removeRouteLayers();
        this.removeTodoLayer();

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

    public showHideRoute() {
        const isRouteShowed = this._showRoute;

        this.removeTrackLayers();
        this.removeTimelapseLayers();
        this.removeRouteLayers();
        this.removeTodoLayer();

        if (this._showYear) {
            this._map.removeLayer(this._yearLayer);
            this._showYear = false;
        }

        if (this._showLast) {
            this._map.removeLayer(this._lastRideLineLayer);
            this._map.removeLayer(this._lastRidePlacesLayer);
            this._showLast = false;
        }

        if (!isRouteShowed) {
            if (this._currentRoutes) {
                this.showRoute(this._currentRoutes, false);
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

    public getShowRoute(): boolean {
        return this._showRoute;
    }

    public isStravaRoute(): boolean {
        if (this._currentRoutes)
        {
            return this._currentRoutes.isStravaRoute;
        }

        return false;
    }

    public isTimelapseRunning(): boolean {
        return this._timelapseRunning;
    }

    public getShowTrackForSelectedPlace(): boolean {
        return this._showTrack
    }

    public getShowProvinces(): boolean {
        return this._showProvinces;
    }

    public showHideProvinces(geoJson: GeoJSON) {
        this._showProvinces = !this._showProvinces;

        const mapStyleHelper = this._mapStyleHelper;

        if (this._showProvinces) {
            if (!this._provincesLayer) {
                const format = new GeoJSONFormat();
                const features = format.readFeatures(geoJson, {
                    featureProjection: 'EPSG:3857'
                });

                this._provincesLayer = new VectorLayer({
                    source: new VectorSource({ features }),
                    style: mapStyleHelper.getRawStyle(MapStyleHelper.LayerStyleDistrictsLine),
                    zIndex: 1000
                });
            }

            this._map.addLayer(this._provincesLayer);
        } else {
            if (this._provincesLayer) {
                this._map.removeLayer(this._provincesLayer);
            }
        }
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

    private removeRouteLayers() {
        if (this._showRoute) {
            if (this._routeLineLayer) {
                this._map.removeLayer(this._routeLineLayer);
            }

            if (this._routePlacesLayer) {
                this._map.removeLayer(this._routePlacesLayer);
            }

            this._showRoute = false;
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

    public removeTodoLayer() {
        if (this._showTodo) {
            if (this._todoLayer) {
                this._map.removeLayer(this._todoLayer);
            }

            this._todoLayer = undefined;
            this._showTodo = false;
        }
    }

    public getShowTodo(): boolean {
        return this._showTodo;
    }

    public showTodoPlace(placeId: string, centerPoint: number[] | null) {
        this.removeTodoLayer();
        this.removeTrackLayers();
        this.removeTimelapseLayers();
        this.removeRouteLayers();

        if (this._showYear) {
            this._map.removeLayer(this._yearLayer);
            this._showYear = false;
        }

        if (this._showLast) {
            this._map.removeLayer(this._lastRideLineLayer);
            this._map.removeLayer(this._lastRidePlacesLayer);
            this._showLast = false;
        }

        const mapStyleHelper = this._mapStyleHelper;
        const map = this._map;

        this._todoLayer = new VectorTileLayer({
            source: this._vectorTileSource,
            style: function (feature) {
                const featurePointer = feature.getProperties()["featurePointer"] as string;
                const id = featurePointer.split(":")[0];
                
                if (id === placeId) {
                    return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleSelectedPlace, map);
                }

                return new Style();
            }
        });

        this._map.addLayer(this._todoLayer);
        this._showTodo = true;

        if (centerPoint) {
            const center = fromLonLat([centerPoint[0], centerPoint[1]]);

            this._map.getView().setCenter(center);
            this._map.getView().setZoom(this.getInitialZoom() + 4);
        }
    }

    public showTrackForSelectedPlace(placeId: string, track: Track, doZoom = true) {
        this.removeTrackLayers();
        this.removeTimelapseLayers();
        this.removeRouteLayers();
        this.removeTodoLayer();

        this._currentTrack = track;
        this._currentPlace = placeId;

        const currentTrack = this._currentTrack;
        const currentPlace = this._currentPlace;
        const mapStyleHelper = this._mapStyleHelper;
        const map = this._map;

        this._trackLayer = new VectorTileLayer({
            source: this._vectorTileSource,
            style: function (feature) {
                const featurePointer = feature.getProperties()["featurePointer"] as string
                const id = featurePointer.split(":")[0];

                if (currentTrack && currentTrack.places && currentTrack.places.some(s => s === id)){
                    if (currentPlace && currentPlace === id) {
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleSelectedPlace, map);
                    } else {
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleVisitedTrack, map);
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
            style: mapStyleHelper.getStyle(MapStyleHelper.LayerStyleTrackLine, undefined)
        });

        this._map.addLayer(this._trackLayer);
        this._map.addLayer(this._lineTrackLayer);
        
        if (doZoom) {
            const trackExtent = this._lineTrackLayer.getSource()?.getFeatures()[0].getGeometry()?.getExtent();

            if (trackExtent) {
                const center = this.getCenter(trackExtent);
                const zoom = this._clientSettings?.trackZoom ?? (this.getInitialZoom() + 1);
    
                this._map.getView().setCenter(center);
                this._map.getView().setZoom(zoom);
            }    
        }
        
        this._showTrack = true;
    }

    public getPlaces(routes: Routes) : string[]{
        let places : string[] = [];

        if (routes && routes.routeCollection){
            routes.routeCollection.forEach(r => {
                if (r.places){
                    places = [...places, ...r.places]
                }
            })
        }

        return places;
    }

    public hasPlaces(routes: Routes) : boolean{
        return this.getPlaces(routes).length > 0;
    }

    public showRoute(routes: Routes, doZoom = true) {
        let places = this.getPlaces(routes);
        let hasTracks = false;

        if (routes && routes.routeCollection){
            routes.routeCollection.forEach(r => {
                hasTracks = hasTracks || (r.coordinates && r.coordinates.length > 1);
            })
        }

        const hasPlacesOnRoutes = places.length > 0;
        const hasTracksOnRoutes = hasTracks;

        if (hasPlacesOnRoutes || hasTracksOnRoutes) {
            this.removeTrackLayers();
            this.removeTimelapseLayers();
            this.removeRouteLayers();
            this.removeTodoLayer();
    
            this._currentRoutes = routes;

            const mapStyleHelper = this._mapStyleHelper;

            const score = this._score;
            const scoreYear = this._scoreYear;
                
            this._routePlacesLayer = new VectorTileLayer({
                source: this._vectorTileSource,
                style: function (feature) {
                    const featurePointer = feature.getProperties()["featurePointer"] as string
                    const id = featurePointer.split(":")[0];
    
                    if (places.some(s => s === id)){
                        const isVisited = score && score.some(v => v.id === id);
                        const isVisitedYear = scoreYear && scoreYear.some(v => v.id === id);

                        if (!isVisited) {
                            return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleNewCheckedPlace, map);
                        } else if (!isVisitedYear) {
                            return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleNewYearCheckedPlace, map)
                        }

                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleCheckedPlace, map);
                    }
    
                    return new Style();
                }
            });

            const lineFeatures : Feature[] = [];
        
            routes.routeCollection.forEach(r => {
                if (r.places){
                    places = [...places, ...r.places]
                }

                if (r.coordinates && r.coordinates.length > 1) {
                    const feature = new Feature({
                        geometry: new LineString(r.coordinates).transform('EPSG:4326', 'EPSG:3857'), 
                    });

                    lineFeatures.push(feature);
                }
            });

            const map = this._map;

            this._routeLineLayer = new VectorLayer({
                source : new VectorSource({
                    features: lineFeatures
                }),
                style: mapStyleHelper.getStyle(MapStyleHelper.LayerStyleTrackLine, undefined)
            });
    
            this._map.addLayer(this._routePlacesLayer);
            this._map.addLayer(this._routeLineLayer);
            
            if (doZoom) {
                const trackExtent = this._routeLineLayer.getSource()?.getFeatures()[0].getGeometry()?.getExtent();
    
                if (trackExtent) {
                    const center = this.getCenter(trackExtent);
        
                    this._map.getView().setCenter(center);
                    this._map.getView().setZoom(this.getInitialZoom() + 1);
                }    
            }
            
            this._showRoute = true;
        }
    }

    private removeNonTimelapseLayers() {
        this.removeTrackLayers();
        this.removeRouteLayers();
        this.removeTodoLayer();
    
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
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleTimelapseLineLast, undefined);
                    } else if (map.getTimelapseIndex() == -1 || index < map.getTimelapseIndex()) {
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleTimelapseLine, undefined);
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
            const map = this._map;

            this._timelapseLayer = new VectorTileLayer({
                source: this._vectorTileSource,
                style: function (feature) {
                    const featurePointer = feature.getProperties()["featurePointer"] as string
                    const id = featurePointer.split(":")[0];

                    if (newPlaces.has(id)){
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleSelectedPlace, map);
                    } else if (places.has(id)){
                        return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleVisitedTrack, map);
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
                    source: new OSM({maxZoom: 17, attributions: "© 2018-2026 LTC2-The MIT License (MIT)-see program folder for licenses | © <a href=\"https://www.openstreetmap.org/copyright\" target=\"_blank\">OpenStreetMap</a> contributors. "})
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
                    return styleHelper.getStyle(MapStyleHelper.LayerStyleVisited, map);
                }

                return styleHelper.getStyle(MapStyleHelper.LayerStyle, map);
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
        const map = this._map;

        const ylayer = new VectorTileLayer({
            source: this._vectorTileSource,
            style: function (feature) {
                const featurePointer = feature.getProperties()["featurePointer"] as string
                const id = featurePointer.split(":")[0];

                if (scoreYear && scoreYear.some(s => s.id === id)){
                    return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleVisitedYear, map);
                }

                return new Style();
            }
        });
        
        return ylayer;
    }

    private initLastRidePlacesLayer(): VectorTileLayer {
        const scoreLast = this._scoreLast;
        const mapStyleHelper = this._mapStyleHelper;
        const map = this._map;

        const llayer = new VectorTileLayer({
            source: this._vectorTileSource,
            style: function (feature) {
                const featurePointer = feature.getProperties()["featurePointer"] as string
                const id = featurePointer.split(":")[0];

                if (scoreLast && scoreLast.some(s => s.id === id)){
                    return mapStyleHelper.getStyle(MapStyleHelper.LayerStyleVisitedYear, map);
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
            style: mapStyleHelper.getStyle(MapStyleHelper.LayerStyleLine, undefined)
        });
        
        return lineLayer;
    }
    
}