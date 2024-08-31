<template>
  <div ref="challengeMap" class="h-full"></div>
  <div ref="popup" class="ol-popup">
    <a href="#" @click="closePopup()" ref="popup-closer" class="ol-popup-closer"></a>
    <div>{{ place }}</div>
  </div>
  <div ref="mapcontrol" style="bottom: 10px; left: .5em; width: 150px;" class="ol-unselectable ol-control">
    <button style="width: 130px; margin:10px; margin-bottom: 3px; font-size: 16px;" @click="onclickDetails()">{{ buttonText }}</button>
    <button style="width: 130px; margin:10px; margin-top: 5px; margin-bottom: 5px; font-size: 16px;" @click="onClickTimelapse">{{ buttonTimelapseText }}</button>
    
    <div v-if="hasYear">
        <input type="checkbox" ref="checkBoxYear" class="focus:ring-0 focus:ring-offset-0 focus:shadow-none" style="margin-left: 10px; margin-right: 2px; vertical-align: middle;position: relative;" @click="onShowHideYear()"><a href="#" style="vertical-align: middle;position: relative;" @click="onShowHideYear()"> {{ buttumYearText }}</a>
    </div>
    <div v-else>
        <p style="margin-left: 10px;">-- {{ buttumYearText }} </p>
    </div>
    
    <input type="checkbox" ref="checkBoxLast"  style="margin-left: 10px; margin-right: 2px; vertical-align: middle;position: relative;" @click="onShowHideLast()"><a href="#" style="vertical-align: middle;position: relative;" @click="onShowHideLast()"> {{ bottumLastText }}</a>
    
    <div v-if="hasTrack">
        <input type="checkbox" ref="checkBoxTrack" class="focus:ring-0 focus:ring-offset-0 focus:shadow-none" style="margin-left: 10px; margin-right: 2px; vertical-align: middle;position: relative;" @click="onShowHideTrackForPlace()"><a href="#" style="vertical-align: middle;position: relative;" @click="onShowHideTrackForPlace()"> {{ currentTrackDate }} </a>
    </div>

    <p style="margin-left: 10px; margin-top: 5px; font-size: 12px;">{{ bottumText }}</p>
  </div>
</template>

<script lang="ts">
import { inject, onMounted, ref, defineComponent, nextTick } from 'vue';
import { AppTypes } from '../types/AppTypes';
import { Track } from '../models/Track';

import { MapHelper } from './helpers/MapHelpers';
import { fromatDateAsYYYYDDMM } from '../utils/Utils';

export default defineComponent({

    emits: ['detailsRequested', 'spinnerRequested' ],

    setup(_, { emit }) {
        const _profileService = inject(AppTypes.IProfileServiceKey);
        const _clientSettings = inject(AppTypes.ClientSettingsKey);
        const _translationService = inject(AppTypes.ITranslationServiceKey);
        
        const challengeMap = ref<HTMLElement>();
        const popup = ref<HTMLElement>();
        const mapcontrol = ref<HTMLDivElement>();
        const checkBoxYear = ref<HTMLInputElement>();
        const checkBoxLast = ref<HTMLInputElement>();
        const checkBoxTrack = ref<HTMLInputElement>();
        const place = ref<string>("");
        const hasYear = ref<boolean>();
        const hasTrack = ref<boolean>();
        const currentTrackDate = ref<string>(""); 
        const currentYear = new Date().getFullYear();

        const buttonText = _translationService?.getText("challengemap.buttonText");
        const bottumText = _translationService?.getText("challengemap.bottumText");
        const buttonTimelapseText = _translationService?.getText("challengemap.buttonTimelapse");

        let mapHelper: MapHelper;
        
        let buttumYearText: string | undefined;
        let bottumLastText: string | undefined;
  
        const score = _profileService?.getProfile()?.placesInAllTimeScore;
        const scoreYear = _profileService?.getProfile()?.placesInYearScore;
        const scoreLast = _profileService?.getProfile()?.placesInLastRideScore;
        
        const yearTotal = (scoreYear?.length ?? 0).toString();
        buttumYearText = _translationService?.getTextViaTemplate("challengemap.buttonYearText", [ currentYear.toString(), yearTotal ]);

        const lastTotal = (scoreLast?.length ?? 0).toString();
        bottumLastText = _translationService?.getTextViaTemplate("challengemap.buttonLastText", [lastTotal ]);

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
                if (checkBoxTrack?.value) {
                    const checkBoxElement = checkBoxTrack.value;                
                    checkBoxElement.checked = mapHelper.getShowTrackForSelectedPlace();
                }
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

        return ({ challengeMap, popup, place, closePopup, mapcontrol, checkBoxYear, checkBoxLast, checkBoxTrack, onclickDetails, buttonText, buttumYearText, buttonTimelapseText, bottumText, bottumLastText, hasYear, onShowHideYear, onShowHideLast, onShowHideTrackForPlace, showTrackForPlace, onClickTimelapse, hasTrack, currentTrackDate } )
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