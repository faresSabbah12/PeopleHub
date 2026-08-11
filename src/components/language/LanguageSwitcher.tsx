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
  const { i18n } = useTranslation();

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
      size='sm'
      onClick={handleLanguageChange}
      aria-label='Change language'
    >
      <Languages />
      {currentLanguage.toUpperCase()}
    </Button>
  );
}
