import { emptyString } from './Constants'
import { StravaRoute } from './StravaRoute'

export class PresentationRoute {

    constructor(stravaRoute: StravaRoute){
        this.id = stravaRoute.routeId;

        if (stravaRoute.timestamp) {
            const dt = new Date(stravaRoute.timestamp * 1000);

            const month = dt.getMonth() + 1;
            const montAsString = month.toString().padStart(2, '0');
            const day = dt.getDay() + 1;
            const dayAsString = day.toString().padStart(2, '0');

            this.date = dt.getFullYear().toString() + '-' + montAsString + '-' + dayAsString;
        } else {
            this.date = '---'
        }

        const distanceInKm = stravaRoute.distance / 1000;

        this.distance = new Intl.NumberFormat('nl-nl', {minimumFractionDigits: 2}).format(distanceInKm) + ' km';

        this.name = stravaRoute.name.substring(0,80) + ' (' + this.distance + ')'; 
    }

    public id: string = emptyString;

    public name: string = emptyString;

    public date: string = emptyString;

    public distance: string =  emptyString;
}