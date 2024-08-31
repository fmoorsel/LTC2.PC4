import type { ITranslationService } from '../interfaces/ITranslationService';

import "reflect-metadata";
import { injectable } from "inversify";
import { gloClientSettings } from '@/models/ClientSettings';

import { emptyString } from '../models/Constants';

import axios from "axios";

@injectable()
export class TranslationService implements ITranslationService {
    
    private _texts : Map<string, string> | undefined;

    getText(id: string): string {
        if (this._texts?.has(id)){
            const value = this._texts?.get(id);
            
            if (value || value === emptyString) {
                return value;
            }
        }

        return id;
    }

    getTextViaTemplate(id: string, parameters: string[]): string {
        if (this._texts?.has(id)){
            let value = this._texts?.get(id);
            
            if (parameters) {
                for(let i = 0; i < parameters.length; i++) {
                    const replacement = parameters[i];
                    const toReplace = "{" + i + "}";
                    
                    if (value || value === emptyString) {
                        value = value.replace(toReplace, replacement)
                    }
                }        
            }

            if (value) {
                return value;
            }
        }

        return id;
    }

    async loadText(lang: string): Promise<void> {
        const url = `./messages/messages.${lang}.json`

        const response = await axios.get(url, { timeout: gloClientSettings.requestTimeout });
        const texts = response.data;

        this._texts = new Map<string, string>();

        let property: keyof typeof texts;
        for (property in texts) {
            this._texts.set(property, texts[property]);
        }
    }
    
}
