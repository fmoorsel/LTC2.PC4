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

            <div class="pb-0 pt-2 px-2 bg-white dark:bg-gray-900">
                <label for="table-search" class="sr-only">Search</label>
                <div class="relative mt-1">
                    <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                        <svg class="w-5 h-5 text-gray-500 dark:text-gray-400" aria-hidden="true" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"><path fill-rule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clip-rule="evenodd"></path></svg>
                    </div>
                    <input type="text" v-model="filter" ref="inputElement" @keyup="keyUp" class="block p-2 pl-10 text-sm text-gray-900 border border-gray-300 rounded-lg w-80 bg-gray-50 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white dark:focus:ring-blue-500 dark:focus:border-blue-500" :placeholder="texthint">
                </div>
            </div>

            <div class="relative overflow-x-auto">
                <div ref="tableContainer" class="p-2 space-y-2 overflow-y-scroll overflow-x-clip mb-4" style="height: 330px;">
                    <table class="w-full text-sm text-left text-gray-500 dark:text-gray-400">
                        <tbody v-if="sortOnNameIndiciator">
                            <tr v-for="visit in sortedVisits" :key="visit.id" @click="doShowPlaceAndRoute(visit.id)" class="bg-white border-b dark:bg-gray-800 dark:border-gray-700 cursor-pointer">
                                <th scope="row" class="px-2 py-2 font-medium text-gray-900 whitespace-nowrap dark:text-white" >
                                   {{ visit.name }}
                                </th>
                                <td class="px-2 py-4">
                                    {{ visit.date }}
                                </td>
                            </tr>
                        </tbody>
                        <tbody v-else>
                            <tr v-for="visit in sortedVisits" :key="visit.id" @click="doShowPlaceAndRoute(visit.id)" class="bg-white border-b dark:bg-gray-800 dark:border-gray-700 cursor-pointer">
                                <th scope="row" class="px-2 py-2 w-4 font-medium text-gray-900 whitespace-nowrap dark:text-white">
                                    {{ visit.date }}
                                </th>
                                <td class="px-6 py-4">
                                    {{ visit.name }}
                                </td>
                            </tr>
                        </tbody>                
                    </table>
                </div>
            </div>
            <!-- Modal footer -->
            <div class="flex items-center p-6 space-x-2 border-t border-gray-200 rounded-b dark:border-gray-600">
                <button @click="sortOnName(true)" type="button" class="text-gray-500 bg-white hover:bg-gray-100 focus:ring-4 focus:outline-none focus:ring-blue-300 rounded-lg border border-gray-200 text-sm font-medium px-5 py-2.5 hover:text-gray-900 focus:z-10 dark:bg-gray-700 dark:text-gray-300 dark:border-gray-500 dark:hover:text-white dark:hover:bg-gray-600 dark:focus:ring-gray-600">{{ buttonOnName }}</button>
                <button @click="sortOnDate(true)" type="button" class="text-gray-500 bg-white hover:bg-gray-100 focus:ring-4 focus:outline-none focus:ring-blue-300 rounded-lg border border-gray-200 text-sm font-medium px-5 py-2.5 hover:text-gray-900 focus:z-10 dark:bg-gray-700 dark:text-gray-300 dark:border-gray-500 dark:hover:text-white dark:hover:bg-gray-600 dark:focus:ring-gray-600">{{ buttonOnDate }}</button>
            </div>
        </div>
    </div>
  </div>  

</template>

<script lang="ts">
import { defineComponent, PropType, ref, onMounted, inject, nextTick } from 'vue';
import { Visit } from '../models/Visit';
import { Modal } from 'flowbite';

import { AppTypes } from '../types/AppTypes';

export default defineComponent({
    props: {
        visits: {
            required: true,
            type: Array as PropType<Visit[]>
        }
    },

    emits: ['trackForPlaceRequested', 'error'],

    setup (props, { emit }) {
        let filteredVisits = [...props.visits];

        const _translationService = inject(AppTypes.ITranslationServiceKey);
        const _profileService = inject(AppTypes.IProfileServiceKey);
       
        const modalElement = ref<HTMLElement>();
        const tableContainer = ref<HTMLDivElement>();
        const inputElement = ref<HTMLInputElement>();
        const sortedVisits = ref(filteredVisits);
        const sortOnNameIndiciator = ref(true);
        const filter = ref("");

        const header = _translationService?.getText("resultsmodal.header");
        const buttonOnName = _translationService?.getText("resultsmodal.button.onname");
        const buttonOnDate = _translationService?.getText("resultsmodal.button.ondate");
        const texthint = _translationService?.getText("resultsmodal.text.hint");

        let modal: Modal;
        let requestingTrackInProgress = false;
 
        onMounted(() => {
            modal = new Modal(modalElement?.value);
        }) 

        const showModal = () => {
            filter.value = '';
            filteredVisits = [...props.visits];

            sortOnName(true);
            
            modal.show();
        }

        const hideModal = () => {
            modal.hide();
        }

        const toTop = () => {
            nextTick(() => {
                if (tableContainer.value) {
                    tableContainer.value.scrollTop = 0;
                }

                if (inputElement.value) {
                    inputElement.value.focus();
                }
            })
        }

        const sortOnDate = (scrollReset: boolean) => {
            const visitsToSort = [...filteredVisits];

            sortedVisits.value = visitsToSort.sort((a, b) => { return a.date > b.date ? 1 : -1})
            sortOnNameIndiciator.value = false;

            if (scrollReset) {
                toTop();
            }
        }

        const sortOnName = (scrollReset: boolean) => {
            sortedVisits.value  = [...filteredVisits].sort((a, b) => { return a.name.toLowerCase() > b.name.toLowerCase() ? 1 : -1 });
            sortOnNameIndiciator.value = true;            

            if (scrollReset) {
                toTop();
            }
        }

        const keyUp = () => {
            doFilter();
        }

        const doFilter = () => {
            if (filter.value && filter.value != '') {
                filteredVisits = [...props.visits].filter(v => { return v.name.toLowerCase().includes(filter.value.toLowerCase()) })
            } else {
                filteredVisits = [...props.visits]
            }

            if (sortOnNameIndiciator.value) {
                sortOnName(false);
            } else {
                sortOnDate(false);
            }
        }

        const doShowPlaceAndRoute = async (placeId: string) => {
            if (!requestingTrackInProgress)
            {
                requestingTrackInProgress = true;

                console.log(placeId);

                try {
                    const track = await _profileService?.getAlltimeTrack(placeId);

                    if (track) {
                        emit('trackForPlaceRequested', placeId, track);
                        
                        hideModal();
                    }
                } catch (error) {
                    console.log("error when selecting track: " + error);
                            
                    emit('error', error);                
                }

                requestingTrackInProgress = false;
            }
        }

        return { sortedVisits, showModal, hideModal, sortOnDate, sortOnName, sortOnNameIndiciator, modalElement, header, buttonOnName, buttonOnDate, tableContainer, texthint, keyUp, filter, inputElement, doShowPlaceAndRoute }
    }
})

</script>

<style lang="scss" scoped>

</style>