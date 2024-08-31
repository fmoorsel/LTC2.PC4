<template>
  <div class="mh-none">  
    <div class="flex h-screen w-full">
      <div class="m-auto">
        <h3 v-if="ismobile">{{ createHSpaceMessageMobile }}</h3>
        <h3 v-else>{{ createHSpaceMessage }}</h3>
      </div>
    </div>
  </div>

  <div class="mw-none">  
    <div class="flex h-screen w-full">
      <div class="m-auto">
        <h3>{{ createWSpaceMessage }}</h3>
      </div>
    </div>
  </div>
  
  <div class="mh-visible">
    <div class="flex flex-col h-screen max-h-screen">
      <div class="flex justify-center relative px-4 pt-6 pb-8">
        <div class="w-full max-w-screen-sm">          
          <div class="text-black text-center text-3xl pb-2">Hi {{ name }}</div>

          <div v-if="hasError" class="p-4 text-sm text-center text-red-800 rounded-lg bg-red-50 dark:bg-gray-800 dark:text-red-400" role="alert">
            <span class="font-medium">{{ errorMessage }}</span>
          </div>
          <div v-else class="p-4 text-sm text-center text-gray-800 rounded-lg bg-gray-50 dark:bg-gray-800 dark:text-gray-300" role="alert">
            <span v-if="profileComplete || isStandalone" class="font-medium">{{ completeMessage }}</span>
            <span v-else class="font-medium">{{ notCompleteMessage }}</span>
          </div>

          <div class="flex justify-center pt-2 space-x-2">
            <button @click="onShowResultClick()" class="block text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800" type="button">
              {{ buttonText }}
            </button>
            <button v-if="isStandalone" @click="onShowProfileClick()" class="block text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800" type="button">
              {{ buttonProfileTextAlt }}
            </button>
            <button v-else @click="onShowProfileClick()" class="block text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800" type="button">
              {{ buttonProfileText }}
            </button>
          </div>
        </div>
      </div>
      <ChallengeMap ref="challengeMap" @detailsRequested="onShowResultClick()" @spinnerRequested="onSpinnerRequested()"/>
      <ResultsModal :visits="visits" ref="resultsModal" @error="onError" @track-for-place-requested="onTrackForPlaceRequested" />
      <ProfileModal ref="profileModal" @profileUpdated="onProfileUpdated()" @error="onError"/>
      <SpinnerModal ref="spinnerModal" />
    </div>
  </div>

</template>

<script lang="ts">
import { inject, ref } from 'vue';
import { AppTypes } from './types/AppTypes';
import { emptyString } from './models/Constants';
import { Visit } from './models/Visit';
import { Track } from './models/Track';
import { IsMobile  } from './utils/Utils';
import { NotAuthorizedException } from './exceptions/NotAuthorizedException';
import { gloClientSettings } from "./models/ClientSettings";

import ChallengeMap from './components/ChallengeMap.vue';
import ResultsModal from './components/ResultsModal.vue';
import ProfileModal from './components/ProfileModal.vue';
import SpinnerModal from './components/SpinnerModal.vue';

export default {
  components: { ChallengeMap, ResultsModal, ProfileModal, SpinnerModal },
  
  setup() {
    const _profileService = inject(AppTypes.IProfileServiceKey);
    const _mapService = inject(AppTypes.IMapServiceKey);
    const _translationService = inject(AppTypes.ITranslationServiceKey);

    const profileVisits = _profileService ? _profileService?.getVisits() : new Array<Visit>();
    
    const profile = _profileService?.getProfile();
    const visits = ref<Visit[]>(profileVisits);

    const checkedPlacesCount = profile?.placesInAllTimeScore.length.toString() ?? "0";
    const placeCount = _mapService?.getPlaceCount().toString() ?? "0";

    const name = ref<string | undefined>(profile?.name)
    const notCompleteMessage = ref<string | undefined>(_translationService?.getText("app.profile.not.complete"))
    const completeMessage = ref<string | undefined>(_translationService?.getTextViaTemplate("app.profile.complete", [checkedPlacesCount, placeCount]))
    const createHSpaceMessage  = ref<string | undefined>(_translationService?.getText("app.profile.createhspace"))
    const createWSpaceMessage  = ref<string | undefined>(_translationService?.getText("app.profile.createwspace"))
    const createHSpaceMessageMobile  = ref<string | undefined>(_translationService?.getText("app.profile.createhspace.mb"))
    const buttonText = ref<string | undefined>(_translationService?.getText("app.buttontext"))
    const buttonProfileText = ref<string | undefined>(_translationService?.getText("app.buttonprofiletext"))
    const buttonProfileTextAlt = ref<string | undefined>(_translationService?.getText("app.buttonprofiletextalt"))
    const profileComplete = ref<boolean>(profile?.email != emptyString);
    const ismobile = ref(IsMobile());
    const hasError =ref<boolean>(false);
    const errorMessage = ref<string | undefined>(_translationService?.getText("app.errortext"));
    const expiredErrorMessage = ref<string | undefined>(_translationService?.getText("app.expiredtext"));

    const isStandalone = ref<boolean | undefined>(gloClientSettings.standaloneVersion);

    console.log(IsMobile());

    const resultsModal = ref<typeof ResultsModal>();
    const profileModal = ref<typeof ProfileModal>();
    const challengeMap = ref<typeof ChallengeMap>();
    const spinnerModal = ref<typeof SpinnerModal>();

    let spinnerActive = false;
    
    let scriptTag = document.createElement("script");
    scriptTag.setAttribute("type", "text/javascript");
    scriptTag.setAttribute("src", "./services/Library.js");
    document.getElementsByTagName("head")[0].appendChild(scriptTag);

    const onShowResultClick = () => {
      resultsModal.value?.showModal();
    }

    const onShowProfileClick = () => {
      profileModal.value?.showModal();
    }

    const onProfileUpdated = () => {
      profileComplete.value = profile?.email != emptyString;
    }

    const onTrackForPlaceRequested = (placeId: string, track:Track) => {
      challengeMap.value?.showTrackForPlace(placeId, track);
    }

    const onSpinnerRequested = () => {
      if (spinnerActive) {
        spinnerModal.value?.hideModal();
      } else {
        spinnerModal.value?.showModal();
      }

      spinnerActive = !spinnerActive;
    }

    const onError = (error: Error) => {
      if (gloClientSettings.mainApplicationForcedLogoutPage === emptyString) {
        window.location.href = "/Home?forceLogout=true"
      } else {
        const isAuthError = error instanceof NotAuthorizedException;
        hasError.value = true;

        if (isAuthError){
          errorMessage.value = expiredErrorMessage.value;

          setTimeout( () => {
            window.location.href = gloClientSettings.mainApplicationForcedLogoutPage;
          }, 2000);
        } else {
          setTimeout( () => {
            hasError.value = false
          }, 2000);
        }
      }
    }
    
    return { name, profileComplete, notCompleteMessage, completeMessage, visits, onError, onShowResultClick, onShowProfileClick, onProfileUpdated, resultsModal, profileModal, buttonText, buttonProfileText, buttonProfileTextAlt, createHSpaceMessage, createHSpaceMessageMobile, createWSpaceMessage, ismobile, hasError, errorMessage, onTrackForPlaceRequested, challengeMap, spinnerModal, onSpinnerRequested, isStandalone }
  }
}
</script>

<style lang="scss">

.mh-none {
    display: none;
}

@media only screen and (max-height: 600px) {
    .mh-none {
        display: block;
    }

    #app {
      z-index: 50;
      position: relative;
      background-color: lightgrey;
    }
}

.mh-visible {
    display: block;
}

@media only screen and (max-height: 600px) {
    .mh-visible {
        display: none;
    }
}

.mw-none {
    display: none;
}

@media only screen and (max-width: 350px) {
    .mw-none {
        display: block;
    }

    #app {
      z-index: 50;
      position: relative;
      background-color: lightgrey;
    }
}

.mw-visible {
    display: block;
}

@media only screen and (max-width: 350px) {
    .mw-visible {
        display: none;
    }
}

</style>
