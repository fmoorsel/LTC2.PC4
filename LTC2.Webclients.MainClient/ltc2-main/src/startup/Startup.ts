import type { ISeriveTask } from "../interfaces/IServiceTask"

import { GetClientSettingsTask } from "../servicetasks/GetClientSettingsTask"
import { RegisterTypesTask } from "../servicetasks/RegisterTypesTask";
import { GetProfileTask } from "../servicetasks/GetProfileTask";
import { GetInitialTranslationsTask } from '../servicetasks/GetInitialTranslationsTask';
import { GetInitMapTask } from '../servicetasks/GetInitMapTask';

import { container } from "../types/AppTypes";
import { TYPES } from "../types/TYPES";

export class Startup{
    
    async startUp(){
        const preRegisterTasks: ISeriveTask[] = [
            new GetClientSettingsTask(), 
            new RegisterTypesTask(),
        ];

        for (const task of preRegisterTasks) {
            await task.execute();
        }

        const postRegisterTasks: ISeriveTask[] = [
            container.get<GetInitialTranslationsTask>(TYPES.GetInitialTranslationsTask),
            container.get<GetInitMapTask>(TYPES.GetInitMapTask),
            container.get<GetProfileTask>(TYPES.GetProfileTask),
        ];

        for (const task of postRegisterTasks) {
            await task.execute();
        }
    }
}