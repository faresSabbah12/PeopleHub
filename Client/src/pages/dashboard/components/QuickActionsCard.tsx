import { useTranslation } from 'react-i18next';

import { SectionCard } from '@/components/common/SectionCard';
import { Button } from '@/components/ui/button';
import { quickActions } from '../dashboardData';

/** Shortcuts to the HR tasks managers start most often. */
export function QuickActionsCard() {
  const { t } = useTranslation('dashboard');

  return (
    <SectionCard title={t('QUICK_ACTIONS')}>
      <div className='grid grid-cols-2 gap-2'>
        {quickActions.map((action) => {
          const Icon = action.icon;

          return (
            <Button
              key={action.id}
              variant='outline'
              size='lg'
              className='h-auto flex-col items-start gap-2 p-3 text-start whitespace-normal'
            >
              <Icon className='text-primary' />
              <span className='text-xs font-medium'>{t(action.labelKey)}</span>
            </Button>
          );
        })}
      </div>
    </SectionCard>
  );
}
