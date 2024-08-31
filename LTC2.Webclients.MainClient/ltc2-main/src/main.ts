import { createApp } from 'vue'
import App from './App.vue'

import { RegisterTypesPlugin } from './plugins/RegisterTypesPlugin'
import { Startup } from './startup/Startup'
import { gloClientSettings } from './models/ClientSettings'
import { emptyString } from './models/Constants'
import { NotAuthorizedException } from './exceptions/NotAuthorizedException'
import './assets/tailwind.css'

const startUp = new Startup();

const startVueApp = function() {
    createApp(App).use(RegisterTypesPlugin).mount('#app')
}

const forceLogout = function(error: Error){
    console.log(`Error: ${error.message}`);

    console.log(gloClientSettings.mainApplicationForcedLogoutPage)
    if (gloClientSettings.mainApplicationForcedLogoutPage === emptyString){
        window.location.href = "/Home?forceLogout=true"
    } else {
        const isAuthError = error instanceof NotAuthorizedException;
        if (isAuthError){
           window.location.href = gloClientSettings.mainApplicationForcedLogoutPage
        } else {
           window.location.href = gloClientSettings.mainApplicationErrorPage
        }
    }
}

startUp.startUp()
    .then(startVueApp)
    .catch(error => forceLogout(error))

