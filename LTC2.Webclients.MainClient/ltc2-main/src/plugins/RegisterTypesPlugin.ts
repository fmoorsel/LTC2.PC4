import { App, Plugin } from 'vue';
import { AppTypes } from '../types/AppTypes';

const RegisterTypesPlugin: Plugin = {
    install(app: App) {
        AppTypes.Register(app);
    }
}

export { RegisterTypesPlugin }