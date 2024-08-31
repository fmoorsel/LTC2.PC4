import type { ISeriveTask } from "../interfaces/IServiceTask";

import { RegisterTypesInContainer } from "../types/AppTypes"

export class RegisterTypesTask implements ISeriveTask{
    async execute(): Promise<void>{
        console.log('RegisterTypesTask');

        RegisterTypesInContainer();
    }
}