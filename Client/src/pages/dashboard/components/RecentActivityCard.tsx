import { useTranslation } from 'react-i18next';

import { SectionCard } from '@/components/common/SectionCard';
import { StatusBadge } from '@/components/common/StatusBadge';
import { statusLabelKeys } from '@/lib/status';
import { UserSummary } from '@/components/common/UserSummary';
import { formatRelativeMinutes } from '@/lib/format';
import { recentActivity } from '../dashboardData';

/** Latest HR actions taken across the workspace. */
export function RecentActivityCard({ className }: { className?: string }) {
  const { t, i18n } = useTranslation('dashboard');

  return (
    <SectionCard
      className={className}
      title={t('RECENT_ACTIVITY')}
      description={t('RECENT_ACTIVITY_DESCRIPTION')}
    >
      <ul className='divide-y divide-border'>
        {recentActivity.map((item) => (
          <li
            key={item.id}
            className='flex items-center justify-between gap-3 py-3 first:pt-0 last:pb-0'
          >
            <UserSummary
              name={item.actor}
              meta={t(item.messageKey)}
              size='sm'
            />

            <div className='flex shrink-0 flex-col items-end gap-1'>
              <StatusBadge
                status={item.status}
                label={t(statusLabelKeys[item.status])}
              />

              <span className='text-xs text-muted-foreground'>
                {formatRelativeMinutes(item.minutesAgo, i18n.language)}
              </span>
            </div>
          </li>
        ))}
      </ul>
    </SectionCard>
  );
}
