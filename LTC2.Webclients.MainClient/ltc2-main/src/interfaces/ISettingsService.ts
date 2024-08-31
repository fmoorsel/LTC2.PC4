import { MainClientSettings } from '../models/MainClientSettings'

export interface ISettingsService {
    getSettings(): Promise<MainClientSettings>;
}
