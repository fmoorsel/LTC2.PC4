import { Route } from './Route'
import { emptyString } from './Constants'
import { LimitsInfo } from './LimitsInfo';

export class Routes {

    public isPlannerRoute: boolean = false;
    public plannerRouteId: string = emptyString;
    public limitInfo : LimitsInfo | undefined = undefined;

    public routeCollection: Route[] = [];

}