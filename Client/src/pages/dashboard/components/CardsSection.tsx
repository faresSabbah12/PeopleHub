import { useTranslation } from 'react-i18next';

import { CardsContainer } from '@/components/common/CardsContainer';
import { InfoCard, type InfoCardProps } from '@/components/common/InfoCard';
import { useApiQuery } from '@/hooks/useApi';
import type { DashboardSummary } from '@/types/dashboard';

import { AttendanceOverviewCard } from './AttendanceOverviewCard';
import { DepartmentBreakdownCard } from './DepartmentBreakdownCard';
import { QuickActionsCard } from './QuickActionsCard';
import { RecentActivityCard } from './RecentActivityCard';
import { TeamStatusCard } from './TeamStatusCard';
import { UpcomingEventsCard } from './UpcomingEventsCard';
import { CalendarClock, CalendarPlus, UsersRound, Wallet } from 'lucide-react';

export function CardsSection() {
  const { t } = useTranslation('dashboard');
  const summary = useApiQuery<DashboardSummary>('/dashboard/summary', {
    method: 'GET',
  });

  const cards: InfoCardProps[] = [
    {
      label: t('STAT_TOTAL_EMPLOYEES'),
      icon: UsersRound,
      value: summary?.headcount.current.toLocaleString() || '',
      tone: 'brand',
      trend: { value: '+3.2%', direction: 'up' },
      hint: t('STAT_VS_LAST_MONTH'),
    },
    {
      label: t('STAT_PRESENT_TODAY'),
      icon: CalendarClock,
      value: summary?.attendanceRate.current.toLocaleString() || '',
      tone: 'success',
      trend: {
        value: `${summary?.attendanceRate.changePercent.toLocaleString() || ''} %`,
        direction:
          (summary?.attendanceRate.changePercent ?? 0) >= 0 ? 'up' : 'down',
      },
      hint: t('STAT_ATTENDANCE_RATE'),
    },
    {
      label: t('STAT_ON_LEAVE'),
      icon: CalendarPlus,
      value: summary?.leave.onLeaveToday.toLocaleString() || '',
      tone: 'warning',
      hint: `${summary?.leave.activeLeaveRequests} ${t('STAT_PENDING_REQUESTS')}`,
    },
    {
      label: t('STAT_MONTHLY_PAYROLL'),
      icon: Wallet,
      value: summary ? `JD ${summary.yearlyPayroll.current}` : '',
      tone: 'info',
      trend: {
        value: `${summary?.yearlyPayroll.changePercent.toLocaleString() || ''} %`,
        direction:
          (summary?.yearlyPayroll.changePercent ?? 0) >= 0 ? 'up' : 'down',
      },
      hint: t('STAT_VS_LAST_MONTH'),
    },
  ];

  return (
    <>
      <CardsContainer columns={4}>
        {cards.map((card) => (
          <InfoCard
            label={card.label}
            icon={card.icon}
            value={card.value}
            tone={card.tone}
            trend={card.trend}
            hint={card.hint}
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
