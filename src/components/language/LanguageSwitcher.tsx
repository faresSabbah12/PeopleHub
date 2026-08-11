import {
  getNextLanguage,
  saveLanguage,
  updateDocumentLanguage,
  type Language,
} from '@/i18n/language';
import { useTranslation } from 'react-i18next';
import { Button } from '../ui/button';
import { Languages } from 'lucide-react';

export function LanguageSwitcher() {
  const { t, i18n } = useTranslation();

  const currentLanguage = i18n.language as Language;

  const handleLanguageChange = async () => {
    const nextLanguage = getNextLanguage(i18n.language as Language);

    saveLanguage(nextLanguage);
    updateDocumentLanguage(nextLanguage);

    await i18n.changeLanguage(nextLanguage);
  };

  return (
    <Button
      variant='ghost'
      size='icon'
      onClick={handleLanguageChange}
      aria-label={t('LANGUAGE_SWITCH')}
      title={t('LANGUAGE_SWITCH')}
      className='w-auto gap-1.5 px-2 text-xs font-medium'
    >
      <Languages />
      {currentLanguage.toUpperCase()}
    </Button>
  );
}
