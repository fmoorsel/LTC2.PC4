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
                <div class="p-2 space-y-2 overflow-y-clip overflow-x-clip mb-4" style="height: 460px;">
                    <p class="pl-2 hidden md:block">{{ name }} ({{ athleteIdLabel }} <a :href="athleteLink" target="_blank">{{ athleteId }}</a>)</p>
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
                    <div v-else>

                        <hr class="h-px my-8 bg-gray-200 border-0 dark:bg-gray-700" style="margin-top: 10px; margin-bottom: 10px;">

                        <p class="pl-2">{{ todoLabel }}</p>

                        <div class="pb-0 pt-2 px-2 bg-white dark:bg-gray-900">
                            <label for="todo-search" class="sr-only">Search</label>
                            <div class="relative mt-1">
                                <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                                    <svg class="w-5 h-5 text-gray-500 dark:text-gray-400" aria-hidden="true" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"><path fill-rule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clip-rule="evenodd"></path></svg>
                                </div>
                                <input type="text" v-model="filter" ref="inputElement" @keyup="keyUp" class="block p-2 pl-10 text-sm text-gray-900 border border-gray-300 rounded-lg w-80 bg-gray-50 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white dark:focus:ring-blue-500 dark:focus:border-blue-500" :placeholder="texthint">
                            </div>
                        </div>

                        <hr class="h-px bg-gray-200 border-0 dark:bg-gray-700" style="margin-top: 10px; margin-bottom: 0px;">

                        <div class="p-2 space-y-2 overflow-y-scroll overflow-x-clip mb-4" style="height: 220px;">
                            <p v-for="(names, lineIndex) in todoLines" :key="lineIndex" class="p-2" style="padding-top: 0px; padding-bottom: 0px; margin: 0px;">
                                <template v-for="(name, nameIndex) in names" :key="name">
                                    <a href="#" @click.prevent="onTodoClick(name)" class="text-blue-600 hover:underline">{{ name }}</a><span v-if="nameIndex < names.length - 1">, </span>
                                </template>
                            </p>
                        </div>
                   
                    </div>

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
import { defineComponent, ref, onMounted, inject, nextTick } from 'vue';
import { Modal } from 'flowbite';

import { AppTypes } from '../types/AppTypes';
import { gloClientSettings } from "../models/ClientSettings";
import { runsInRideWithGpsMode } from "../utils/Utils";

export default defineComponent ({
    
    emits: ['profileUpdated', 'error', 'todoSelected'],

    setup (_, { emit }) {
        const _profileService = inject(AppTypes.IProfileServiceKey);
        const _mapService = inject(AppTypes.IMapServiceKey);
        const _translationService = inject(AppTypes.ITranslationServiceKey);
        
        const profile = _profileService?.getProfile();
        const checkedPlacesCount = profile?.placesInAllTimeScore.length.toString() ?? "0";
        const placeCount = _mapService?.getPlaceCount().toString() ?? "0";
        const lastrideTimestamp = profile?.mostRecentVisitDate ?? "--";
        const toDoCountNumber = (_mapService?.getPlaceCount() ?? 0) - (profile?.placesInAllTimeScore.length ?? 0);
        const toDoCount = toDoCountNumber >= 0 ?  toDoCountNumber.toString() : '--';

        const visits = _profileService?.getVisits()?.map(v => v.name) ?? [];
        const toDos = _mapService?.getGroupedNotCheckedPlaces(80, visits) ?? [];

        const sortedToDos = ref(toDos);
        const allTodoNames = toDos.map(line => line.split(', ').map(n => n.trim()).filter(n => n)).flat();
        const todoLines = ref(toDos.map(line => line.split(', ').map(n => n.trim()).filter(n => n)));

        const filter = ref("");
        const inputElement = ref<HTMLInputElement>();

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
        const todoLabel = _translationService?.getTextViaTemplate("profilemodal.todoLabel", [toDoCount]);
        const texthint = _translationService?.getText("resultsmodal.text.hint");

        const name = profile?.name;
        const email = profile?.email;
        const clientId = "Client Id " + profile?.clientId;

        const isNotStandalone = ref<boolean | undefined>(!gloClientSettings.standaloneVersion);

        const athleteId = profile?.athleteId;
        const athleteIdLabel = runsInRideWithGpsMode() ? "Ride with Gps ID" : "Strava ID";
        const athleteLink = "https://www.strava.com/athletes/" + athleteId;

        let modal: Modal;
 
        onMounted(() => {
            modal = new Modal(modalElement?.value);

            if (email && emailInput?.value) {
                emailInput.value.value = email;
            }
        }) 

        const showModal = () => {
            filter.value = '';
            todoLines.value = toDos.map(line => line.split(', ').map(n => n.trim()).filter(n => n));
            modal.show();
            nextTick(() => {
                if (isNotStandalone.value) {
                    emailInput.value?.focus();
                } else {
                    inputElement.value?.focus();
                }
            });
        }

        const groupIntoLines = (names: string[], maxLength: number): string[][] => {
            const lines: string[][] = [];
            let currentLine: string[] = [];
            let currentLength = 0;
            for (const name of names) {
                const addLength = currentLine.length === 0 ? name.length : 2 + name.length;
                if (currentLine.length > 0 && currentLength + addLength > maxLength) {
                    lines.push(currentLine);
                    currentLine = [name];
                    currentLength = name.length;
                } else {
                    currentLine.push(name);
                    currentLength += addLength;
                }
            }
            if (currentLine.length > 0) {
                lines.push(currentLine);
            }
            return lines;
        }

        const keyUp = () => {
            if (filter.value && filter.value !== '') {
                const filtered = allTodoNames.filter(n => n.toLowerCase().includes(filter.value.toLowerCase()));
                todoLines.value = groupIntoLines(filtered, 80);
            } else {
                todoLines.value = toDos.map(line => line.split(', ').map(n => n.trim()).filter(n => n));
            }
        }

        const hideModal = () => {
            modal.hide();
        }

        const onTodoClick = (name: string) => {
            emit('todoSelected', name);
            hideModal();
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

        return { showModal, hideModal, submitForm, validateEmail, modalElement, header, name, athleteId, athleteIdLabel, athleteLink, clientId, scoreLine, lastRideLine, scoreLineShort, lastRideLineShort, emailInput, emailLabel, emailForm, emailPlaceholder, buttonSave, buttonClose, isNotStandalone, todoLabel, sortedToDos, todoLines, onTodoClick, filter, inputElement, texthint, keyUp }
    }
})

</script>

<style>

</style>