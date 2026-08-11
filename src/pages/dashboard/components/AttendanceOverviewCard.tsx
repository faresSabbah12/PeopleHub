import { useTranslation } from 'react-i18next';

import { SectionCard } from '@/components/common/SectionCard';
import { StatusBadge } from '@/components/common/StatusBadge';
import { statusLabelKeys } from '@/lib/status';
import { Button } from '@/components/ui/button';
import { formatNumber } from '@/lib/format';
import { attendanceBreakdown, attendanceWeek } from '../dashboardData';

/** Weekly attendance rate plus today's status split. */
export function AttendanceOverviewCard({ className }: { className?: string }) {
  const { t, i18n } = useTranslation('dashboard');

  return (
    <SectionCard
      className={className}
      title={t('ATTENDANCE_OVERVIEW')}
      description={t('ATTENDANCE_OVERVIEW_DESCRIPTION')}
      action={
        <Button variant='ghost' size='sm'>
          {t('VIEW_REPORT')}
        </Button>
      }
    >
      <div className='flex h-40 items-end gap-2 sm:gap-3'>
        {attendanceWeek.map((day) => (
          <div
            key={day.dayKey}
            className='flex h-full flex-1 flex-col items-center justify-end gap-2'
          >
            <span className='text-xs font-medium text-muted-foreground'>
              {day.presentRate}%
            </span>

            <div
              className='w-full rounded-md bg-primary/85 transition-[height] hover:bg-primary'
              style={{ height: `${day.presentRate}%` }}
              role='img'
              aria-label={`${t(day.dayKey)}: ${day.presentRate}%`}
            />

            <span className='text-xs text-muted-foreground'>
              {t(day.dayKey)}
            </span>
          </div>
        ))}
      </div>

      <div className='mt-5 grid grid-cols-2 gap-3 border-t border-border pt-4 sm:grid-cols-4'>
        {attendanceBreakdown.map((item) => (
          <div key={item.status} className='space-y-1.5'>
            <StatusBadge
              status={item.status}
              label={t(statusLabelKeys[item.status])}
            />
            <p className='font-heading text-lg font-semibold'>
              {formatNumber(item.count, i18n.language)}
            </p>
          </div>
        ))}
      </div>
    </SectionCard>
  );
}
