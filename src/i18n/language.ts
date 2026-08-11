export const DEFAULT_LANGUAGE = 'en';

export const SUPPORTED_LANGUAGES = ['en', 'ar'] as const;

export type Language = (typeof SUPPORTED_LANGUAGES)[number];

export const LANGUAGE_STORAGE_KEY = 'peoplehub-language';

/**
 * Checks whether a value is a supported application language.
 */
export const isSupportedLanguage = (language: string): language is Language =>
  SUPPORTED_LANGUAGES.includes(language as Language);

/**
 * Returns the currently saved language.
 * Falls back to English when no valid language is stored.
 */
export const getCurrentLanguage = (): Language => {
  const storedLanguage = localStorage.getItem(LANGUAGE_STORAGE_KEY);

  if (storedLanguage && isSupportedLanguage(storedLanguage)) {
    return storedLanguage;
  }

  return DEFAULT_LANGUAGE;
};

/**
 * Saves the selected language.
 */
export const saveLanguage = (language: Language): void => {
  localStorage.setItem(LANGUAGE_STORAGE_KEY, language);
};

/**
 * Returns the document direction for a language.
 */
export const getDirection = (language: Language): 'ltr' | 'rtl' =>
  language === 'ar' ? 'rtl' : 'ltr';

/**
 * Updates the language and direction on the HTML element.
 */
export const updateDocumentLanguage = (language: Language): void => {
  document.documentElement.lang = language;
  document.documentElement.dir = getDirection(language);
};

/**
 * Returns the opposite language.
 */
export const getNextLanguage = (language: Language): Language =>
  language === 'en' ? 'ar' : 'en';
