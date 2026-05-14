<template>
  <div ref="modalElement" tabindex="-1" aria-hidden="true" class="fixed top-0 left-0 right-0 z-50 hidden w-full p-4 overflow-x-hidden overflow-y-auto md:inset-0 h-modal md:h-full">
    <div class="relative w-full h-auto max-w-2xl">
      <div class="relative bg-white rounded-lg shadow dark:bg-gray-700">
        <!-- Modal header -->
        <div class="flex items-start justify-between p-4 border-b rounded-t dark:border-gray-600">
          <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
            {{ modalHeader }}
          </h3>
          <button type="button" @click="hideModal()" class="text-gray-400 bg-transparent hover:bg-gray-200 hover:text-gray-900 rounded-lg text-sm p-1.5 ml-auto inline-flex items-center dark:hover:bg-gray-600 dark:hover:text-white">
            <svg aria-hidden="true" class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"><path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"></path></svg>
            <span class="sr-only">Close modal</span>
          </button>
        </div>
        <!-- Modal body -->
        <div class="relative overflow-x-auto">
          <div class="p-2 overflow-y-clip overflow-x-clip mb-4" style="height: 355px;">

            <p class="pl-2 pt-1 font-medium text-gray-900 dark:text-white">{{ headerParts.before }}<a href="#" @click.prevent="onAreaClick(placeName)" class="text-blue-600 hover:underline font-medium">{{ placeName }}</a>{{ headerParts.after }}</p>

            <hr class="h-px bg-gray-200 border-0 dark:bg-gray-700" style="margin-top: 12px; margin-bottom: 8px;">

            <p class="pl-2" style="margin-top: 16px;">{{ alltimePanelLabel }}</p>

            <div class="p-2 overflow-y-scroll overflow-x-clip" style="height: 110px;">
              <p v-for="(names, lineIndex) in alltimeLines" :key="lineIndex" style="padding: 0px; margin: 0px;">
                <template v-for="(name, nameIndex) in names" :key="name">
                  <a href="#" @click.prevent="onAreaClick(name)" class="text-blue-600 hover:underline">{{ name }}</a><span v-if="nameIndex < names.length - 1">, </span>
                </template>
              </p>
            </div>

            <hr class="h-px bg-gray-200 border-0 dark:bg-gray-700" style="margin-top: 8px; margin-bottom: 8px;">

            <p class="pl-2" style="margin-top: 16px;">{{ yearPanelLabel }}</p>

            <div class="p-2 overflow-y-scroll overflow-x-clip" style="height: 110px;">
              <p v-for="(names, lineIndex) in yearLines" :key="lineIndex" style="padding: 0px; margin: 0px;">
                <template v-for="(name, nameIndex) in names" :key="name">
                  <a href="#" @click.prevent="onAreaClick(name)" class="text-blue-600 hover:underline">{{ name }}</a><span v-if="nameIndex < names.length - 1">, </span>
                </template>
              </p>
            </div>

          </div>
        </div>
        <!-- Modal footer -->
        <div class="flex items-center p-4 space-x-2 border-t border-gray-200 rounded-b dark:border-gray-600">
          <button type="button" @click="hideModal()" class="block text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800">{{ buttonClose }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, PropType, ref, onMounted, inject, computed } from 'vue';
import { Modal } from 'flowbite';
import { AppTypes } from '../types/AppTypes';

export default defineComponent({
    props: {
        placeName: {
            required: true,
            type: String
        },
        isNewAlltime: {
            required: true,
            type: Boolean
        },
        isNewYear: {
            required: true,
            type: Boolean
        },
        newAlltimeNames: {
            required: true,
            type: Array as PropType<string[]>
        },
        newYearNames: {
            required: true,
            type: Array as PropType<string[]>
        }
    },

    emits: ['areaSelected'],

    setup(props, { emit }) {
        const _translationService = inject(AppTypes.ITranslationServiceKey);

        const modalElement = ref<HTMLElement>();
        let modal: Modal;

        const modalHeader = _translationService?.getText("areadetailmodal.header");
        const alltimePanelLabelBase = _translationService?.getText("areadetailmodal.panel.alltime.label") ?? '';
        const yearPanelLabelBase = _translationService?.getText("areadetailmodal.panel.year.label") ?? '';

        const alltimePanelLabel = computed(() => `${alltimePanelLabelBase} (${props.newAlltimeNames.length})`);
        const yearPanelLabel = computed(() => `${yearPanelLabelBase} (${props.newYearNames.length})`);
        const buttonClose = _translationService?.getText("areadetailmodal.button.close");

        onMounted(() => {
            modal = new Modal(modalElement?.value);
        });

        const headerParts = computed(() => {
            let template: string;
            if (props.isNewAlltime) {
                template = _translationService?.getText("areadetailmodal.header.newalltime") ?? '{0}';
            } else if (props.isNewYear) {
                template = _translationService?.getText("areadetailmodal.header.newyear") ?? '{0}';
            } else {
                template = _translationService?.getText("areadetailmodal.header.existing") ?? '{0}';
            }
            const idx = template.indexOf('{0}');
            return {
                before: idx >= 0 ? template.slice(0, idx) : template,
                after: idx >= 0 ? template.slice(idx + 3) : ''
            };
        });

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
        };

        const alltimeLines = computed(() => groupIntoLines([...props.newAlltimeNames].sort(), 80));
        const yearLines = computed(() => groupIntoLines([...props.newYearNames].sort(), 80));

        const showModal = () => {
            modal.show();
        };

        const hideModal = () => {
            modal.hide();
        };

        const onAreaClick = (name: string) => {
            emit('areaSelected', name);
            hideModal();
        };

        return { modalElement, modalHeader, headerParts, alltimePanelLabel, yearPanelLabel, buttonClose, showModal, hideModal, onAreaClick, alltimeLines, yearLines };
    }
});
</script>

<style>
</style>
