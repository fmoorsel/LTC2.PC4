import { PresentationRoutes } from '../models/PresentationRoutes';
import { Routes } from '../models/Routes'

export interface IRouteCheckerService {
    checkGpx(file: File):  Promise<Routes | undefined>;

    listRoutes(source?: string): Promise<PresentationRoutes | undefined>;

    checkRoute(routeId: string, source?: string): Promise<Routes | undefined>;

    checkGpxFromPath(file: string): Promise<Routes | undefined>;
}