export interface ITranslationService {
    getText(id: string): string;
    getTextViaTemplate(id: string, parameters: string[]): string;
    loadText(lang: string): Promise<void>;
}