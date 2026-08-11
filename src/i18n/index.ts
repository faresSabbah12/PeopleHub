import i18n from 'i18next';
import HttpBackend from 'i18next-http-backend';
import { initReactI18next } from 'react-i18next';

import {
  defaultLanguage,
  defaultNamespace,
  supportedLanguages,
} from './config';

import { getCurrentLanguage, updateDocumentLanguage } from './language';

i18n
  .use(HttpBackend) // Loads translation files from public/locales/ en & ar
  .use(initReactI18next)
  .init({
    lng: getCurrentLanguage(),

    fallbackLng: defaultLanguage,
    supportedLngs: supportedLanguages,
    defaultNS: defaultNamespace,

    ns: [defaultNamespace], // other namespaces are fetched on demand via useTranslation('ns')

    interpolation: {
      escapeValue: false,
    },

    backend: {
      loadPath: '/locales/{{lng}}/{{ns}}.json',
    },
  });

updateDocumentLanguage(getCurrentLanguage());

export default i18n;
