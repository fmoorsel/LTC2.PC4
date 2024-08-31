import { App } from 'vue';
import { InjectionKey } from 'vue';
import { Container } from "inversify";

import type { IProfileService } from '../interfaces/IProfileService';
import type { ISettingsService } from '../interfaces/ISettingsService';
import type { ITranslationService } from '../interfaces/ITranslationService';
import type { IMapService } from '../interfaces/IMapService';

import { ProfileService } from '../services/ProfileService';
import { SettingsService } from '../services/SettingsService';
import { ClientSettings } from '../models/ClientSettings';
import { gloClientSettings } from '../models/ClientSettings';
import { GetProfileTask } from '../servicetasks/GetProfileTask';
import { TranslationService } from '../services/TranslationService';
import { GetInitialTranslationsTask } from '../servicetasks/GetInitialTranslationsTask';
import { MapService } from '../services/MapService';
import { GetInitMapTask } from '../servicetasks/GetInitMapTask';

import { TYPES } from './TYPES';

const container = new Container();

class AppTypes {
    static ClientSettingsKey : InjectionKey<ClientSettings> = TYPES.ClientSettings
    static IProfileServiceKey : InjectionKey<IProfileService> = TYPES.IProfileService
    static ISettingsServiceKey : InjectionKey<ISettingsService> = TYPES.ISettingsService
    static ITranslationServiceKey : InjectionKey<ITranslationService> = TYPES.ITranslationService
    static IMapServiceKey : InjectionKey<IMapService> = TYPES.IMapService

    static Register (app: App) {
        RegisterTypesToVue(app);
    }
}

function RegisterTypesToVue(app: App) {
    app.provide(AppTypes.ClientSettingsKey, container.get<ClientSettings>(TYPES.ClientSettings))
    app.provide(AppTypes.IProfileServiceKey, container.get<IProfileService>(TYPES.IProfileService))
    app.provide(AppTypes.ISettingsServiceKey, container.get<ISettingsService>(TYPES.ISettingsService))
    app.provide(AppTypes.ITranslationServiceKey, container.get<ITranslationService>(TYPES.ITranslationService))
    app.provide(AppTypes.IMapServiceKey, container.get<IMapService>(TYPES.IMapService))
}

function RegisterTypesInContainer() {
    container.bind<ClientSettings>(TYPES.ClientSettings).toConstantValue(gloClientSettings);
    container.bind<GetProfileTask>(TYPES.GetProfileTask).to(GetProfileTask);
    container.bind<GetInitialTranslationsTask>(TYPES.GetInitialTranslationsTask).to(GetInitialTranslationsTask);
    container.bind<GetInitMapTask>(TYPES.GetInitMapTask).to(GetInitMapTask);
    container.bind<IProfileService>(TYPES.IProfileService).to(ProfileService).inSingletonScope();
    container.bind<ISettingsService>(TYPES.ISettingsService).to(SettingsService).inSingletonScope();
    container.bind<ITranslationService>(TYPES.ITranslationService).to(TranslationService).inSingletonScope();
    container.bind<IMapService>(TYPES.IMapService).to(MapService).inSingletonScope();
}

export { AppTypes, RegisterTypesInContainer, container };