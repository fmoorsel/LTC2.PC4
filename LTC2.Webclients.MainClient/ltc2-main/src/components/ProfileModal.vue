<template>

<div ref="modalElement" tabindex="-1" aria-hidden="true"  class="fixed top-0 left-0 right-0 z-50 hidden w-full p-4 overflow-x-hidden overflow-y-auto md:inset-0 h-modal md:h-full">
    <!-- div class="relative w-full h-full max-w-2xl md:h-auto" -->
    <div class="relative w-full h-auto max-w-2xl">
        <!-- Modal content -->
        <div class="relative bg-white rounded-lg shadow dark:bg-gray-700">
            <!-- Modal header -->
            <div class="flex items-start justify-between p-4 border-b rounded-t dark:border-gray-600">
                <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
                    {{ header }}
                </h3>
                <button type="button" @click="hideModal()" class="text-gray-400 bg-transparent hover:bg-gray-200 hover:text-gray-900 rounded-lg text-sm p-1.5 ml-auto inline-flex items-center dark:hover:bg-gray-600 dark:hover:text-white" >
                    <svg aria-hidden="true" class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"><path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"></path></svg>
                    <span class="sr-only">Close modal</span>
                </button>
            </div>
            <!-- Modal body -->
            <div class="relative overflow-x-auto">
                <div class="p-2 space-y-2 overflow-y-clip overflow-x-clip mb-4" style="height: 330px;">
                    <p class="pl-2 hidden md:block">{{ name }} (Strava ID <a :href="athleteLink" target="_blank">{{ athleteId }}</a>)</p>
                    <p class="pl-2 hidden md:block">{{ clientId }}</p>
                    <p class="pl-2 hidden md:block">{{ scoreLine }}</p>
                    <p class="pl-2 hidden md:block">{{ lastRideLine }}</p>

                    <p class="pl-2 md:hidden">{{ name }} (<a :href="athleteLink" target="_blank">{{ athleteId }}</a>)</p>
                    <p class="pl-2 md:hidden">{{ clientId }}</p>
                    <p class="pl-2 md:hidden">{{ scoreLineShort }}</p>
                    <p class="pl-2 md:hidden">{{ lastRideLineShort }}</p>
                    
                    <form v-if="isNotStandalone" ref="emailForm">
                        <div class="pl-2 pr-2 mt-4 border-t">
                            <label for="email" class="block mb-2 mt-4 text-sm font-medium text-gray-900 dark:text-white">{{ emailLabel}}</label>
                            <input type="email" ref="emailInput" @input="validateEmail()" @invalid="validateEmail()" class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white dark:focus:ring-blue-500 dark:focus:border-blue-500" :placeholder="emailPlaceholder" required>
                        </div>
                    </form> 
                </div>
            </div>
            <div v-if="isNotStandalone" class="flex items-center p-4 space-x-2 border-t border-gray-200 rounded-b dark:border-gray-600">
                <button type="button" @click="submitForm()" class="block text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800">{{ buttonSave }} </button>
            </div>
            <div v-else class="flex items-center p-4 space-x-2 border-t border-gray-200 rounded-b dark:border-gray-600">
                <button type="button" @click="hideModal()" class="block text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800">{{ buttonClose }} </button>
            </div>
        </div>
    </div>
  </div>  

</template>

<script lang="ts">
import { defineComponent, ref, onMounted, inject } from 'vue';
import { Modal } from 'flowbite';

import { AppTypes } from '../types/AppTypes';
import { gloClientSettings } from "../models/ClientSettings";


export default defineComponent ({
    
    emits: ['profileUpdated', 'error'],

    setup (_, { emit }) {
        const _profileService = inject(AppTypes.IProfileServiceKey);
        const _mapService = inject(AppTypes.IMapServiceKey);
        const _translationService = inject(AppTypes.ITranslationServiceKey);
        
        const profile = _profileService?.getProfile();
        const checkedPlacesCount = profile?.placesInAllTimeScore.length.toString() ?? "0";
        const placeCount = _mapService?.getPlaceCount().toString() ?? "0";
        const lastrideTimestamp = profile?.mostRecentVisitDate ?? "--"

        const modalElement = ref<HTMLElement>();
        const emailForm = ref<HTMLFormElement>();
        const emailInput = ref<HTMLInputElement>();

        const header = _translationService?.getText("profilemodal.header");
        const invalidEmail = _translationService?.getText("profilemodal.invalidemail");
        const scoreLine = _translationService?.getTextViaTemplate("profilemodal.score", [checkedPlacesCount, placeCount]);
        const lastRideLine = _translationService?.getTextViaTemplate("profilemodal.lastride", [lastrideTimestamp]);
        const scoreLineShort = _translationService?.getTextViaTemplate("profilemodal.score.sm", [checkedPlacesCount, placeCount]);
        const lastRideLineShort = _translationService?.getTextViaTemplate("profilemodal.lastride.sm", [lastrideTimestamp]);
        const emailLabel = _translationService?.getText("profilemodal.email.label");
        const emailPlaceholder = _translationService?.getText("profilemodal.email.placeholder");
        const buttonSave = _translationService?.getText("profilemodal.button.save");
        const buttonClose = _translationService?.getText("profilemodal.button.close");

        const name = profile?.name;
        const email = profile?.email;
        const clientId = "Client Id " + profile?.clientId;

        const isNotStandalone = ref<boolean | undefined>(!gloClientSettings.standaloneVersion);

        const athleteId = profile?.athleteId;
        const athleteLink = "https://www.strava.com/athletes/" + athleteId;

        let modal: Modal;
 
        onMounted(() => {
            modal = new Modal(modalElement?.value);

            if (email && emailInput?.value) {
                emailInput.value.value = email;
            }
        }) 

        const showModal = () => {
            modal.show();
        }

        const hideModal = () => {
            modal.hide();
        }

        const submitForm = async () => {
            if (emailForm.value?.reportValidity()) {
                if (emailInput.value?.value) {
                    const email = emailInput.value?.value

                    console.log("update email address with " + email);

                    try {
                        if (email && email != profile?.email) {
                            await _profileService?.updateEmail(email);

                            if (profile) {
                                profile.email = email;
                            }

                            emit('profileUpdated');
                        }
                    } catch(error) {
                        console.log("updating email address failed: " + error);
                        
                        if (profile) {
                            emailInput.value.value = profile.email;
                        }

                        emit('error', error);
                    }

                    hideModal();
                }
            }
        }

        const validateEmail = () => {
            if (invalidEmail) {
                if (emailInput.value?.value === "" || emailInput.value?.validity.typeMismatch) {
                    emailInput.value?.setCustomValidity(invalidEmail);
                } else {
                    emailInput.value?.setCustomValidity("");
                }
            }

            return true;
        }

        return { showModal, hideModal, submitForm, validateEmail, modalElement, header, name, athleteId, athleteLink, clientId, scoreLine, lastRideLine, scoreLineShort, lastRideLineShort, emailInput, emailLabel, emailForm, emailPlaceholder, buttonSave, buttonClose, isNotStandalone }
    }
})

</script>

<style>

</style>