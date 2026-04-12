<template>
  <div ref="challengeMap" class="h-full"></div>
  <div ref="popup" class="ol-popup">
    <a href="#" @click="closePopup()" ref="popup-closer" class="ol-popup-closer"></a>
    <div>{{ place }}</div>
  </div>
  <div ref="mapcontrol" style="bottom: 10px; left: .5em; width: 170px;" class="ol-unselectable ol-control">
    <button v-if="enablePlanRoute" style="width: 150px; margin:10px; margin-bottom: 3px; font-size: 16px;" @click="onclickPlanRoute()">{{ buttonPlanRouteText }}</button>
    <button style="width: 150px; margin:10px; margin-top: 5px; margin-bottom: 3px; font-size: 16px;" @click="onclickCheckRoute()">{{ buttonCheckRouteText }}</button>
    <button v-if="isStravaRoute" style="width: 150px; margin:10px; margin-top: 5px; margin-bottom: 3px; font-size: 16px;" @click="onclickReloadRoute()">{{ buttonReloadRouteText }}</button>
    
    <button style="width: 150px; margin:10px; margin-top: 5px; margin-bottom: 3px; font-size: 16px;" @click="onclickDetails()">{{ buttonText }}</button>
    <button style="width: 150px; margin:10px; margin-top: 5px; margin-bottom: 5px; font-size: 16px;" @click="onClickTimelapse">{{ buttonTimelapseText }}</button>

    <div v-if="hasYear">
        <input type="checkbox" ref="checkBoxYear" class="focus:ring-0 focus:ring-offset-0 focus:shadow-none" style="margin-left: 10px; margin-right: 2px; vertical-align: middle;position: relative;" @click="onShowHideYear()"><a href="#" style="vertical-align: middle;position: relative;" @click="onShowHideYear()"> {{ bottumYearText }}</a>
    </div>
    <div v-else>
        <p style="margin-left: 10px;">-- {{ bottumYearText }} </p>
    </div>
    
    <input type="checkbox" ref="checkBoxLast"  class="focus:ring-0 focus:ring-offset-0 focus:shadow-none" style="margin-left: 10px; margin-right: 2px; vertical-align: middle;position: relative;" @click="onShowHideLast()"><a href="#" style="vertical-align: middle;position: relative;" @click="onShowHideLast()"> {{ bottumLastText }}</a>
    
    <div v-if="hasTrack">
        <input type="checkbox" ref="checkBoxTrack" class="focus:ring-0 focus:ring-offset-0 focus:shadow-none" style="margin-left: 10px; margin-right: 2px; vertical-align: middle;position: relative;" @click="onShowHideTrackForPlace()"><a href="#" style="vertical-align: middle;position: relative;" @click="onShowHideTrackForPlace()"> {{ currentTrackDate }} </a>
    </div>

    <div v-if="hasRoutes">
        <input type="checkbox" ref="checkBoxRoute" class="focus:ring-0 focus:ring-offset-0 focus:shadow-none" style="margin-left: 10px; margin-right: 2px; vertical-align: middle;position: relative;" @click="onShowHideRoute()"><a href="#" style="vertical-align: middle;position: relative;" @click="onShowHideRoute()"> {{ buttonRouteText }} </a>
    </div>

    <div>
        <input type="checkbox" ref="checkBoxProvinces" class="focus:ring-0 focus:ring-offset-0 focus:shadow-none" style="margin-left: 10px; margin-right: 2px; vertical-align: middle;position: relative;" @click="onShowHideProvinces()"><a href="#" style="vertical-align: middle;position: relative;" @click="onShowHideProvinces()"> {{ bottumProvinciesText }}</a>
    </div>

    <div>
        <input type="checkbox" ref="checkBoxAllRides" class="focus:ring-0 focus:ring-offset-0 focus:shadow-none" style="margin-left: 10px; margin-right: 2px; vertical-align: middle;position: relative;" @click="onShowHideAllRides()"><a href="#" style="vertical-align: middle;position: relative;" @click="onShowHideAllRides()"> {{ buttonAllRidesText }}</a>
    </div>

    <p style="margin-left: 10px; margin-top: 5px; font-size: 12px;">{{ bottumText }}</p>
  </div>
</template>

<script lang="ts">
import { inject, onMounted, ref, defineComponent, nextTick } from 'vue';
import { AppTypes } from '../types/AppTypes';
import { Track } from '../models/Track';
import { Routes } from '../models/Routes';

import { MapHelper } from './helpers/MapHelpers';
import { fromatDateAsYYYYDDMM, runsInRideWithGpsMode } from '../utils/Utils';

export default defineComponent({

    emits: ['detailsRequested', 'spinnerRequested', 'routeSelectionRequested', 'error' ],

    setup(_, { emit }) {
        const _profileService = inject(AppTypes.IProfileServiceKey);
        const _clientSettings = inject(AppTypes.ClientSettingsKey);
        const _translationService = inject(AppTypes.ITranslationServiceKey);
        const _routeCheckerService = inject(AppTypes.IRouteCheckerService);
        const _mapService = inject(AppTypes.IMapServiceKey);
        
        const challengeMap = ref<HTMLElement>();
        const popup = ref<HTMLElement>();
        const mapcontrol = ref<HTMLDivElement>();
        const checkBoxYear = ref<HTMLInputElement>();
        const checkBoxLast = ref<HTMLInputElement>();
        const checkBoxTrack = ref<HTMLInputElement>();
        const checkBoxRoute = ref<HTMLInputElement>();
        const checkBoxProvinces = ref<HTMLInputElement>();
        const checkBoxAllRides = ref<HTMLInputElement>();
        const place = ref<string>("");
        const hasYear = ref<boolean>();
        const hasTrack = ref<boolean>();
        const hasRoutes = ref<boolean>();
        const isStravaRoute = ref<boolean>();
        const currentTrackDate = ref<string>(""); 
        const currentYear = new Date().getFullYear();

        const buttonText = _translationService?.getText("challengemap.buttonText");
        const buttonPlanRouteText = _translationService?.getText("challengemap.buttonPlanRouteText");
        const buttonCheckRouteText = _translationService?.getText("challengemap.buttonCheckRouteText");
        const bottumText = _translationService?.getText("challengemap.bottumText");
        const buttonTimelapseText = _translationService?.getText("challengemap.buttonTimelapse");
        const buttonReloadRouteText = _translationService?.getText("challengemap.buttonReloadRouteText");

        let mapHelper: MapHelper;
        let _allTimeTracks: Track[] | undefined;
        
        const score = _profileService?.getProfile()?.placesInAllTimeScore;
        const scoreYear = _profileService?.getProfile()?.placesInYearScore;
        const scoreLast = _profileService?.getProfile()?.placesInLastRideScore;

        const yearTotal = (scoreYear?.length ?? 0).toString();
        const bottonYearText = _translationService?.getTextViaTemplate("challengemap.buttonYearText", [ currentYear.toString(), yearTotal ]);

        const lastTotal = (scoreLast?.length ?? 0).toString();
        const buttonLastText = _translationService?.getTextViaTemplate("challengemap.buttonLastText", [lastTotal ]);

        const buttonRouteText =  _translationService?.getText("challengemap.buttonRouteText")
        const bottumProvinciesText = _translationService?.getText("challengemap.buttonProvinciesText")
        const buttonAllRidesText = _translationService?.getText("challengemap.buttonAllRidesText")

        const enablePlanRoute = true;

        hasYear.value = scoreYear && scoreYear.length > 0;

        onMounted(() => {
            const coordinates =  _profileService?.getProfile()?.trackLastRide ?? []; 

            mapHelper = new MapHelper(challengeMap.value, popup.value, mapcontrol.value, score, scoreYear, scoreLast, coordinates, place, _clientSettings);            
        })
    
        const closePopup = () => {
            mapHelper.closePopup();
        }
        
        const onclickDetails = () => {
            console.log("onclickDetails");
            
            if (document.fullscreenElement) {
                document.exitFullscreen();
            }

            emit('detailsRequested')
        }

        const onclickReloadRoute = async () => {
            console.log("onclickReloadRoute");
            
            if (document.fullscreenElement) {
                document.exitFullscreen();
            }

            if (mapHelper.isTimelapseRunning()) {
                mapHelper.performTimelapse(undefined);
            }
            
            emit('spinnerRequested');

            try {
                const routes = mapHelper.getCurrentRoutes();

                if (routes && routes.isStravaRoute) {
                    const newRoutes = await _routeCheckerService?.checkRoute(routes.stravaRouteId);

                    if (newRoutes?.routeCollection && newRoutes.routeCollection.length > 0) {
                        showRoute(newRoutes, false);
                    }
                }
            } catch (error) {
                console.log("error when selecting track: " + error);

                emit('error', error);                     
            } finally {
                emit('spinnerRequested');
            } 
        }

        const onclickPlanRoute = () => {
            console.log("onclickPlanRoute");
            
            const provider = runsInRideWithGpsMode() ? "ridewithgps" : "strava";

            console.log("route planner url: " + provider);

            (window as any).postHostMessage("routeplanner", provider);
        }

        const onclickCheckRoute = () => {
            console.log("onclickCheckRoute");

            if (document.fullscreenElement) {
                document.exitFullscreen();
            }

            emit('routeSelectionRequested')
        }

        const doCheckBoxes = (which: number) => {            
            if (checkBoxYear?.value) {
                const checkBoxElement = checkBoxYear.value;                
                checkBoxElement.checked = which == 1 ? mapHelper.getShowYear() : false;
            }

            if (checkBoxLast?.value) {
                const checkBoxElement = checkBoxLast.value;
                checkBoxElement.checked = which == 2 ? mapHelper.getShowLastRide() : false;
            }

            if (checkBoxTrack?.value) {
                const checkBoxElement = checkBoxTrack.value;
                checkBoxElement.checked = which == 3 ? mapHelper.getShowTrackForSelectedPlace() : false;
            }

            if (checkBoxRoute?.value) {
                const checkBoxElement = checkBoxRoute.value;
                checkBoxElement.checked = which == 4 ? mapHelper.getShowRoute() : false;
            }

            if (checkBoxAllRides?.value) {
                checkBoxAllRides.value.checked = which == 5 ? mapHelper.getShowAllRides() : false;
            }
        }

        const onShowHideYear = () => {
            mapHelper.showHideYear();

            doCheckBoxes(1);
        }

        const onShowHideLast = () => {
            mapHelper.showHideLastRide();

            doCheckBoxes(2);
        }

        const onShowHideTrackForPlace = () => {
            mapHelper.showHideTrackForSelectedPlace();

            doCheckBoxes(3);
        }

        const onShowHideRoute = () => {
            mapHelper.showHideRoute();

            doCheckBoxes(4);
        }

        const onShowHideProvinces = () => {
            const geoJson = _mapService?.getDistrictsMap();

            if (geoJson) {
                mapHelper.showHideProvinces(geoJson);
            }

            if (checkBoxProvinces.value) {
                checkBoxProvinces.value.checked = mapHelper.getShowProvinces();
            }
        }

        const onShowHideAllRides = async () => {
            if (mapHelper.getShowAllRides()) {
                mapHelper.showHideAllRides([]);
                doCheckBoxes(0);
            } else {
                emit('spinnerRequested');

                try {
                    if (!_allTimeTracks) {
                        _allTimeTracks = await _profileService?.getAlltimeTracks();
                    } else {
                        await nextTick();
                    }

                    if (_allTimeTracks) {
                        mapHelper.showHideAllRides(_allTimeTracks);
                        doCheckBoxes(5);
                    }
                } catch (error) {
                    console.log("error when loading all rides: " + error);

                    emit('error', error);
                } finally {
                    emit('spinnerRequested');
                }
            }
        }

        const showRoute = (routes: Routes, doZoom = true) => {
            console.log('show route');

            if (mapHelper.getShowLastRide()) {
                onShowHideLast();
            }

            if (mapHelper.getShowYear()) {
                onShowHideYear();
            }
            
            mapHelper.showRoute(routes, doZoom);

            hasRoutes.value = mapHelper.getShowRoute();
            isStravaRoute.value = mapHelper.isStravaRoute();
        
            nextTick(() => {
                doCheckBoxes(4);
            });
        }

        const showTodoPlace = (placeId: string, centerPoint: number[] | null) => {
            if (mapHelper.isTimelapseRunning()) {
                mapHelper.performTimelapse(undefined);
            }

            mapHelper.showTodoPlace(placeId, centerPoint);

            doCheckBoxes(0);
        }

        const showTrackForPlace = (placeId: string, track: Track) => {
            if (mapHelper.getShowLastRide()) {
                onShowHideLast();
            }

            if (mapHelper.getShowYear()) {
                onShowHideYear();
            }

            mapHelper.showTrackForSelectedPlace(placeId, track);

            hasTrack.value = mapHelper.getShowTrackForSelectedPlace();
            const currTrack = mapHelper.getCurrentTrack();
            currentTrackDate.value = fromatDateAsYYYYDDMM(currTrack?.visitedOn ?? "");

            nextTick(() => {
                doCheckBoxes(3);
            });
        }

        const onClickTimelapse = async () => {
            console.log("onClickTimelapse");

            emit('spinnerRequested')

            try {
                if (mapHelper.isTimelapseRunning()) {
                    mapHelper.performTimelapse(undefined);
                } else {
                    mapHelper.removeTimelapseLayers();

                    const tracks =  await _profileService?.getAlltimeTracks();

                    if (tracks) {
                        doCheckBoxes(0);

                        mapHelper.performTimelapse(tracks);
                    }
                }

                doCheckBoxes(1);
            } finally {
                emit('spinnerRequested')
            }
        }

        return ({ challengeMap, popup, place, closePopup, mapcontrol, checkBoxYear, checkBoxLast, checkBoxTrack, checkBoxRoute, checkBoxProvinces, checkBoxAllRides, onclickDetails, onclickPlanRoute, onclickCheckRoute, buttonText, buttonReloadRouteText, buttonPlanRouteText, buttonCheckRouteText, buttonRouteText, bottumYearText: bottonYearText, buttonTimelapseText, bottumText, bottumLastText: buttonLastText, bottumProvinciesText, buttonAllRidesText, hasYear, onShowHideYear, onShowHideLast, onShowHideTrackForPlace, onShowHideRoute, onShowHideProvinces, onShowHideAllRides, showTrackForPlace, showTodoPlace, onClickTimelapse, hasTrack, currentTrackDate, showRoute, hasRoutes, isStravaRoute, onclickReloadRoute, enablePlanRoute } )
    }
})
</script>

<style>
        .ol-popup {
            font-family: 'Lucida Grande', Verdana, Geneva, Lucida, Arial, Helvetica, sans-serif !important;
            font-size: 12px;
            position: absolute;
            background-color: white;
            -webkit-filter: drop-shadow(0 1px 4px rgba(0, 0, 0, 0.2));
            filter: drop-shadow(0 1px 4px rgba(0, 0, 0, 0.2));
            padding: 15px;
            border-radius: 10px;
            border: 1px solid #cccccc;
            bottom: 12px;
            left: -50px;
            min-width: 200px;
            text-align: center;
        }

        .ol-popup:after,
        .ol-popup:before {
            top: 100%;
            border: solid transparent;
            content: " ";
            height: 0;
            width: 0;
            position: absolute;
            pointer-events: none;
        }

        .ol-popup:after {
            border-top-color: white;
            border-width: 10px;
            left: 48px;
            margin-left: -10px;
        }

        .ol-popup:before {
            border-top-color: #cccccc;
            border-width: 11px;
            left: 48px;
            margin-left: -11px;
        }

        .ol-popup-closer {
            text-decoration: none;
            position: absolute;
            top: 2px;
            right: 8px;
        }

        .ol-popup-closer:after {
            content: "✖";
            color: #c3c3c3;
        }

        .checkboxnb :focus {
            outline: none  !important;;
        }
 
</style>