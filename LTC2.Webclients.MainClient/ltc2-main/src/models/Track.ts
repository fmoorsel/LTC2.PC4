import { emptyString } from './Constants'

export class Track {
    public externalId: string = emptyString;
    public name: string = emptyString;
    public coordinates: number[][] = [];
    public distance: number = 0;
    public visitedOn: string = emptyString;
    public places: string[] = [];
}