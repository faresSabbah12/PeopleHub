import { useTranslation } from 'react-i18next';

import { CardsContainer } from '@/components/common/CardsContainer';
import { StatCard } from '@/components/common/StatCard';

import { AttendanceOverviewCard } from './AttendanceOverviewCard';
import { DepartmentBreakdownCard } from './DepartmentBreakdownCard';
import { QuickActionsCard } from './QuickActionsCard';
import { RecentActivityCard } from './RecentActivityCard';
import { TeamStatusCard } from './TeamStatusCard';
import { UpcomingEventsCard } from './UpcomingEventsCard';
import { dashboardStats } from '../dashboardData';

/** All Card rows shown on the dashboard: stat tiles, then paired overview cards. */
export function CardsSection() {
  const { t } = useTranslation('dashboard');

  return (
    <>
      <CardsContainer columns={4}>
        {dashboardStats.map((stat) => (
          <StatCard
            key={stat.id}
            label={t(stat.labelKey)}
            value={stat.value}
            icon={stat.icon}
            tone={stat.tone}
            trend={stat.trend}
            hint={stat.hintKey ? t(stat.hintKey) : undefined}
          />
        ))}
      </CardsContainer>

      <CardsContainer columns={3}>
        <AttendanceOverviewCard className='lg:col-span-2' />
        <DepartmentBreakdownCard />
      </CardsContainer>

      <CardsContainer columns={3}>
        <TeamStatusCard className='lg:col-span-2' />
        <UpcomingEventsCard />
      </CardsContainer>

      <CardsContainer columns={3}>
        <RecentActivityCard className='lg:col-span-2' />
        <QuickActionsCard />
      </CardsContainer>
    </>
  );
}
