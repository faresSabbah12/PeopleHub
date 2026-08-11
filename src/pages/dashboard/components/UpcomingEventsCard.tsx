import { CalendarDays } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { SectionCard } from '@/components/common/SectionCard';
import { Badge } from '@/components/ui/badge';
import { formatMediumDate } from '@/lib/format';
import { upcomingEvents } from '../dashboardData';

/** Upcoming HR milestones (payroll cut-off, reviews, onboarding). */
export function UpcomingEventsCard() {
  const { t, i18n } = useTranslation('dashboard');

  const events = [...upcomingEvents].sort(
    (a, b) => a.countdownDays - b.countdownDays,
  );

  return (
    <SectionCard
      title={t('UPCOMING_EVENTS')}
      description={t('UPCOMING_EVENTS_DESCRIPTION')}
    >
      <ul className='space-y-3'>
        {events.map((event) => (
          <li
            key={event.id}
            className='flex items-center gap-3 rounded-lg border border-border bg-muted/40 p-3'
          >
            <span className='flex size-9 shrink-0 items-center justify-center rounded-md bg-card text-muted-foreground shadow-2xs'>
              <CalendarDays className='size-4' aria-hidden />
            </span>

            <div className='min-w-0 flex-1'>
              <p className='truncate text-sm font-medium'>
                {t(event.titleKey)}
              </p>
              <p className='text-xs text-muted-foreground'>
                {formatMediumDate(event.date, i18n.language)}
              </p>
            </div>

            <Badge variant={event.countdownDays <= 7 ? 'warning' : 'secondary'}>
              {t('IN_DAYS', { days: event.countdownDays })}
            </Badge>
          </li>
        ))}
      </ul>
    </SectionCard>
  );
}
