import { Download, UserPlus } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { PageHeader } from '@/components/common/PageHeader';
import { Button } from '@/components/ui/button';
import { currentUser } from '@/data/currentUser';

import { CardsSection } from './components/CardsSection';

export function DashboardPage() {
  const { t } = useTranslation('dashboard');

  return (
    <div className='mx-auto flex w-full max-w-[100rem] flex-col gap-4 md:gap-5'>
      <PageHeader
        title={t('GREETING', { name: currentUser.name.split(' ')[0] })}
        description={t('DASHBOARD_SUBTITLE')}
        actions={
          <>
            <Button variant='outline' size='lg'>
              <Download />
              <span className='hidden sm:inline'>{t('EXPORT')}</span>
            </Button>

            <Button size='lg'>
              <UserPlus />
              {t('ADD_EMPLOYEE')}
            </Button>
          </>
        }
      />

      <CardsSection />
    </div>
  );
}
