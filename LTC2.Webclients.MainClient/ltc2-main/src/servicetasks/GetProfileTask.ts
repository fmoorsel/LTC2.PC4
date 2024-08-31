import type { ISeriveTask } from "../interfaces/IServiceTask";
import type { IProfileService } from "../interfaces/IProfileService";

import { injectable, inject } from "inversify";
import "reflect-metadata";
import { TYPES } from '../types/TYPES';
import { emptyString } from "../models/Constants";
import {NotAuthorizedException} from "../exceptions/NotAuthorizedException"

@injectable()
export class GetProfileTask implements ISeriveTask {
    
    @inject(TYPES.IProfileService) 
    private _profileService?: IProfileService

    async execute(): Promise<void>{
        const token = await this._profileService?.getToken();
        
        if (token === emptyString) {
            throw new NotAuthorizedException("We don't have a token!")
        }
        
        const profile = await this._profileService?.loadProfile();

        console.log(token);
        console.log(profile?.name);
    }
}