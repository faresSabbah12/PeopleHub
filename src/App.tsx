import { Button } from '@/components/ui/button';
import { useTranslation } from 'react-i18next';

export default function App() {
  const { t } = useTranslation();
  return (
    <div className='flex min-h-screen items-center justify-center'>
      <Button>PeopleHub</Button>
      <h1>{t('WELCOME')}</h1>;
    </div>
  );
}
