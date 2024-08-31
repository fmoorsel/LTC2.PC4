import { emptyString } from './Constants'
import { Visit } from './Visit'

export class Profile {
    public name: string = emptyString;
    public athleteId: string = emptyString;
    public email: string = emptyString;
    public clientId: string = emptyString;

    public placesInAllTimeScore: Visit[] = [];
    public placesInYearScore: Visit[] = [];
    public placesInLastRideScore: Visit[] = [];
    public trackLastRide: number[][] = [];

    public mostRecentVisitDate : string = emptyString;
}
