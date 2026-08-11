export const defaultLanguage = 'en';

export const supportedLanguages = ['en', 'ar'] as const;

export type Language = (typeof supportedLanguages)[number];

export const defaultNamespace = 'common';
