import { Profile } from '../models/Profile'
import { Visit } from '../models/Visit';
import { Track } from '../models/Track';

export interface IProfileService {
    getToken(): Promise<string>;
    loadProfile(): Promise<Profile>;
    getProfile(): Profile | undefined;
    getVisits(): Visit[];
    updateEmail(email: string): Promise<void>;
    getAlltimeTrack(placeId: string): Promise<Track>;
    getAlltimeTracks(): Promise<Track[]>;    
}
