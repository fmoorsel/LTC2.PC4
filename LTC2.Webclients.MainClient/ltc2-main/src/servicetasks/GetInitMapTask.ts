import type { ISeriveTask } from "../interfaces/IServiceTask";
import type { IMapService } from "../interfaces/IMapService";

import { injectable, inject } from "inversify";
import "reflect-metadata";
import { TYPES } from '../types/TYPES';

@injectable()
export class GetInitMapTask implements ISeriveTask {
    
    @inject(TYPES.IMapService) 
    private _mapService?: IMapService

    async execute(): Promise<void>{
        console.log('GetInitMapTask');

        await this._mapService?.loadMap();
    }
}