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
                  <div style="margin-left: 35px;" class="mt-1 flex items-center space-x-2">                                                
                    <input type="file" accept=".gpx" class="hidden" id="fileInput" @change="onSelectFile">
                    <button @click="onSelectFileButtonClick" class="text-gray-500 bg-white hover:bg-gray-100 focus:ring-4 focus:outline-none focus:ring-blue-300 rounded-lg border border-gray-200 text-sm font-medium px-5 py-2.5 hover:text-gray-900 focus:z-10 dark:bg-gray-700 dark:text-gray-300 dark:border-gray-500 dark:hover:text-white dark:hover:bg-gray-600 dark:focus:ring-gray-600" type="button">{{buttonTextSelectFile}}</button>                    
                    <input type="text" disabled v-model="fileName" ref="inputElement" class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-blue-500 focus:border-blue-500 block w-80 p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white dark:focus:ring-blue-500 dark:focus:border-blue-500" :placeholder="texthint">                            
                    <button v-if="isButtonDisabled" disabled class="text-white bg-blue-300 rounded outline-none font-medium rounded-lg text-sm px-5 py-2.5" type="button">{{buttonTextCheckGpx}}</button>
                    <button v-else @click="onSelectGpx" ref="selectFileButton" class="block text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800" type="button">{{buttonTextCheckGpx}}</button>
                  </div>
                  <div class="flex justify-center p-2 space-y-2 overflow-y-clip overflow-x-clip mb-4" style="height: 30px; margin-bottom: 10px;">
                        <p v-if="isEmpty" class="pl-2 hidden md:block">{{ feedBackNoRoutes }}</p>
                        <p v-else-if="isWorking" class="pl-2 hidden md:block">{{ feedBackWorking }}</p>
                        <p v-else class="pl-2 hidden md:block">{{ feedBackInstuction }}</p>
                  </div>
              </div>
  
              <template v-if="!isRideWithGpsMode">

              <hr class="h-px my-8 bg-gray-200 border-0 dark:bg-gray-700" style="margin-top: 10px; margin-bottom: 10px;">

              <div class="relative overflow-x-auto text-center" >
                  <button ref="loadRoutesButton" @click="onLoadRoutes" class="text-gray-500 bg-white hover:bg-gray-100 focus:ring-4 focus:outline-none focus:ring-blue-300 rounded-lg border border-gray-200 text-sm font-medium px-5 py-2.5 hover:text-gray-900 focus:z-10 dark:bg-gray-700 dark:text-gray-300 dark:border-gray-500 dark:hover:text-white dark:hover:bg-gray-600 dark:focus:ring-gray-600 inline-flex items-center" type="button" style="margin-top: 4px;">
                    <svg class="fill-current w-4 h-4 mr-2" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20"><path d="M13 8V2H7v6H2l8 8 8-8h-5zM0 18h20v2H0v-2z"/></svg>
                    <span>{{buttonTextLoadStravaRoute}}</span>
                  </button>

                  <div ref="tableContainer" class="p-2 space-y-2 overflow-y-scroll overflow-x-clip mb-4" style="height: 310px;">
                    <p v-if="!isRoutesLoaded" style="margin-top: 120px; margin-left: 10px;">{{routesNotYetLoadedText}}</p>
                    <p v-else-if="isLoadingStravaRoutes" style="margin-top: 120px; margin-left: 10px;">{{loadingStravaRoutesText}}</p>
                    <p v-else-if="isNoStravaRoutes" style="margin-top: 120px; margin-left: 10px;">{{noRoutesInStravaText}}</p>
                    <p v-else-if="isStravaRouteLoading" style="margin-top: 120px; margin-left: 10px;">{{loadingStravaRouteText}}</p>
                    <p v-else-if="isNoPlaces" style="margin-top: 120px; margin-left: 10px;">{{noPlacesText}}</p>
                    
                    <table v-else class="w-full text-sm text-left text-gray-500 dark:text-gray-400">
                        <tbody>
                            <tr v-for="route in sortedRoutes" :key="route.id" @click="onSelectRoute(route.id)" class="bg-white border-b dark:bg-gray-800 dark:border-gray-700 cursor-pointer">
                                <th scope="row" class="px-2 py-2  w-4 font-medium text-gray-900 whitespace-nowrap dark:text-white" >
                                   {{ route.date }}
                                </th>
                                <td class="px-2 py-4">
                                    {{ route.name }}
                                </td>
                            </tr>
                        </tbody>
                    
                    </table>

                  </div>
              </div>

              </template>

              <!-- Modal footer -->
              <div class="flex items-center p-6 space-x-2 border-t border-gray-200 rounded-b dark:border-gray-600">
              </div>
          </div>
      </div>
    </div>  
  
  </template>
  
  <script lang="ts">
  import { defineComponent, ref, onMounted, inject, nextTick } from 'vue';
  import { Modal } from 'flowbite';
  
  import { AppTypes } from '../types/AppTypes';
  import { Routes } from '../models/Routes';
  import { emptyString } from '@/models/Constants';
  import { runsInRideWithGpsMode } from '@/utils/Utils';

  import { PresentationRoute } from '../models/PresentationRoute';
  
  interface HTMLInputEvent extends Event {
    target: HTMLInputElement & EventTarget
  }

  interface FileToUpload {
    onlyFileName: string;
    fileName: string;
  }

  declare global {
        interface Window {
        webkit?: {
            messageHandlers: {
            [x:string]: {
                postMessage: (data: string) => void;
            }
            }
        }
    }
  }

  export default defineComponent({
  
      emits: [ 'error', 'routeRequested' ],
  
      setup (_, { emit }) {
  
          const _translationService = inject(AppTypes.ITranslationServiceKey);
          const _routeCheckerService = inject(AppTypes.IRouteCheckerService);
         
          const modalElement = ref<HTMLElement>();
          const tableContainer = ref<HTMLDivElement>();
          const inputElement = ref<HTMLInputElement>();
          const selectFileButton = ref<HTMLButtonElement>();
          const loadRoutesButton = ref<HTMLButtonElement>();
          const fileName = ref(emptyString);
          const feedBackNoRoutes =  _translationService?.getText("selectroutemodal.text.noroutes");
          const feedBackWorking =  _translationService?.getText("selectroutemodal.text.working");
          const feedBackInstuction =  _translationService?.getText("selectroutemodal.text.instruction");

          const isRideWithGpsMode = runsInRideWithGpsMode();

          const header = isRideWithGpsMode
              ? _translationService?.getText("selectroutemodal.header.ridewithgps")
              : _translationService?.getText("selectroutemodal.header");
          const texthint = _translationService?.getText("selectroutemodal.text.hint");
          const buttonTextSelectFile = _translationService?.getText("selectroutemodal.button.selectfile");
          const buttonTextCheckGpx = _translationService?.getText("selectroutemodal.button.checkgpx");
          const buttonTextLoadStravaRoute = _translationService?.getText("selectroutemodal.button.loadstravaroutes");

          const routesNotYetLoadedText = _translationService?.getText("selectroutemodal.text.routesNotYetLoadedText");
          const noRoutesInStravaText = _translationService?.getText("selectroutemodal.text.noRoutesInStrava");
          const loadingStravaRoutesText = _translationService?.getText("selectroutemodal.text.loadingStravaRoutesText");
          const loadingStravaRouteText = _translationService?.getText("selectroutemodal.text.loadingStravaRouteText");
          const noPlacesText = _translationService?.getText("selectroutemodal.text.noplaces");
  
          const isButtonDisabled = ref(true);
          const isEmpty = ref(false);
          const isWorking = ref(false);
          const isRoutesLoaded = ref(false);
          const isNoStravaRoutes = ref(false);
          const isLoadingStravaRoutes = ref(false);
          const isStravaRouteLoading = ref(false);
          const isNoPlaces = ref(false);

          let stravaRoutes :  PresentationRoute[] = new Array<PresentationRoute>();

          const sortedRoutes = ref(stravaRoutes);

          let modal: Modal;
          let selectedFiles: FileList | undefined;
          let selectedBypassFile: FileToUpload | undefined;
         
          onMounted(() => {
              modal = new Modal(modalElement?.value);

              document.addEventListener('onFileEvent', function(event) {
                const fileToUploadJson = (event as CustomEvent).detail['message'];

                var file = JSON.parse(fileToUploadJson) as FileToUpload;

                fileName.value = file.onlyFileName;
                
                selectedBypassFile = file;
                selectedFiles = undefined;

                isButtonDisabled.value = false;
                isEmpty.value = false;

                nextTick(() => {
                    selectFileButton.value?.focus();
                })
              })
          }) 

          const onSelectFileButtonClick = () => {
            if (window.webkit) {
                window.webkit.messageHandlers.webview.postMessage("selectFile");
            } else {
                document.getElementById('fileInput')?.click();
            }
          }
  
          const showModal = () => {
            isButtonDisabled.value = fileName.value == emptyString;
            isEmpty.value = false;
            isWorking.value = false;
            isLoadingStravaRoutes.value = false;
            isNoPlaces.value = false;

            isLoadingStravaRoutes.value = false;

            if (loadRoutesButton?.value) {
                loadRoutesButton.value.disabled = false;
            }
            
            modal.show();
          }
  
          const hideModal = () => {
              modal.hide();
          }

          const onSelectRoute = async (id: string) => {
            console.log('onLoadRoutes');

            if (loadRoutesButton?.value && loadRoutesButton.value.disabled) {
                return;
            }

            isButtonDisabled.value = true;            

            if (loadRoutesButton?.value) {
                loadRoutesButton.value.disabled = true;
            }
            
            try {
                isStravaRouteLoading.value = true;

                var route = await _routeCheckerService?.checkRoute(id);

                if (route && hasPlaces(route)) {
                    emit('routeRequested', route);

                    hideModal();
                }
                else {
                    isNoPlaces.value = true;

                    setTimeout( () => {
                        isNoPlaces.value = false
                    }, 2500);
                }                        
            }
            catch (error) {
                console.log("error when selecting track: " + error);
                            
                hideModal();

                emit('error', error);                     
            }

            isStravaRouteLoading.value = false;
            isButtonDisabled.value = fileName.value == emptyString;

            if (loadRoutesButton?.value) {
                loadRoutesButton.value.disabled = false;
            }
          }

          const onSelectFile = (event: Event) => {
            let files = (event as HTMLInputEvent).target.files;

            if (!files?.length) {
                return;
            }

            console.log("selectFile: " + files[0].name)

            if (files && files.length > 0 && files[0].name) {
                fileName.value = files[0].name;

                selectedFiles = files;
                selectedBypassFile = undefined;

                isButtonDisabled.value = false;
                isEmpty.value = false;

                nextTick(() => {
                    selectFileButton.value?.focus();
                })
            }
          }

          const getPlaces = (routes: Routes) => {
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

          const hasPlaces = (routes: Routes) => {
            return getPlaces(routes).length > 0;
          }

          const onLoadRoutes = async () => {
            console.log('onLoadRoutes');

            isButtonDisabled.value = true;            

            if (loadRoutesButton?.value) {
                loadRoutesButton.value.disabled = true;
            }

            try {
                isLoadingStravaRoutes.value = true;
                isRoutesLoaded.value = true;
                
                const routes = await _routeCheckerService?.listRoutes();

                isLoadingStravaRoutes.value = false;

                const routeCount = routes?.routes?.length ?? 0;
                
                if (routeCount <= 0) {
                    isNoStravaRoutes.value = true;
                }
                
                sortedRoutes.value = routes?.routes ?? new Array<PresentationRoute>();
            }
            catch (error) {
                console.log("error when selecting track: " + error);
                            
                hideModal();

                emit('error', error);                     
            }

            isButtonDisabled.value = fileName.value == emptyString;

            if (loadRoutesButton?.value) {
                loadRoutesButton.value.disabled = false;
            }
           
          }

          const onSelectGpx = async () => {
            if (selectedBypassFile) {
                return onSelectGpxFromPath();
            } else {
                isButtonDisabled.value = true;            
                isWorking.value = true;

                if (loadRoutesButton?.value) {
                    loadRoutesButton.value.disabled = true;
                }

                let files = selectedFiles;

                if (!files?.length) {
                    return;
                }

                try {
                    var route = await _routeCheckerService?.checkGpx(files[0]);

                    if (route && hasPlaces(route)) {
                        emit('routeRequested', route);

                        hideModal();
                    }
                    else {
                        isEmpty.value = true;
                    }                        
                }
                catch (error) {
                    console.log("error when selecting track: " + error);
                                
                    hideModal();

                    emit('error', error);                     
                }

                isButtonDisabled.value = false;
                isWorking.value = false;
                
                if (loadRoutesButton?.value) {
                    loadRoutesButton.value.disabled = false;
                }   
            }
          }
    
          const onSelectGpxFromPath = async () => {
            isButtonDisabled.value = true;            
            isWorking.value = true;

            if (loadRoutesButton?.value) {
                loadRoutesButton.value.disabled = true;
            }

            try {

                if (selectedBypassFile) {
                    var route = await _routeCheckerService?.checkGpxFromPath(selectedBypassFile?.fileName);

                    if (route && hasPlaces(route)) {
                        emit('routeRequested', route);

                        hideModal();
                    }
                    else {
                        isEmpty.value = true;
                    }                      
                }
                      
            }
            catch (error) {
                console.log("error when selecting track: " + error);
                            
                hideModal();

                emit('error', error);                     
            }

            isButtonDisabled.value = false;
            isWorking.value = false;
            
            if (loadRoutesButton?.value) {
                loadRoutesButton.value.disabled = false;
            }
          }
 
          return { showModal, hideModal, modalElement, header, tableContainer, texthint, onSelectFile, inputElement, fileName, buttonTextSelectFile, selectFileButton, isButtonDisabled, onSelectGpx, buttonTextCheckGpx, feedBackNoRoutes, feedBackWorking, isEmpty, isWorking, feedBackInstuction, buttonTextLoadStravaRoute, isRoutesLoaded, routesNotYetLoadedText, noRoutesInStravaText, isNoStravaRoutes, loadRoutesButton, onLoadRoutes, isLoadingStravaRoutes, loadingStravaRoutesText, sortedRoutes, loadingStravaRouteText, isStravaRouteLoading, isNoPlaces, noPlacesText, onSelectRoute, onSelectFileButtonClick, isRideWithGpsMode }
      }
  })
  
  </script>
  
  <style lang="scss" scoped>
  
  </style>
