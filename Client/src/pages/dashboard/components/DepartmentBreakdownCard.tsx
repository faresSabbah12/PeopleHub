import { useTranslation } from 'react-i18next';

import { SectionCard } from '@/components/common/SectionCard';
import { Progress } from '@/components/ui/progress';
import { formatNumber } from '@/lib/format';
import { departments } from '../dashboardData';

/** Headcount distribution across departments. */
export function DepartmentBreakdownCard() {
  const { t, i18n } = useTranslation('dashboard');

  return (
    <SectionCard
      title={t('DEPARTMENT_OVERVIEW')}
      description={t('DEPARTMENT_OVERVIEW_DESCRIPTION')}
    >
      <ul className='space-y-4'>
        {departments.map((department) => (
          <li key={department.id} className='space-y-2'>
            <div className='flex items-baseline justify-between gap-2 text-sm'>
              <span className='truncate font-medium'>
                {t(department.nameKey)}
              </span>

              <span className='shrink-0 text-muted-foreground'>
                {t('EMPLOYEE_COUNT', {
                  formatted: formatNumber(department.headcount, i18n.language),
                })}
              </span>
            </div>

            <Progress
              value={department.share}
              aria-label={t(department.nameKey)}
            />
          </li>
        ))}
      </ul>
    </SectionCard>
  );
}
