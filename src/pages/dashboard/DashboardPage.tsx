import { Download, UserPlus } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { PageHeader } from '@/components/common/PageHeader';
import { StatCard } from '@/components/common/StatCard';
import { Button } from '@/components/ui/button';
import { currentUser } from '@/data/currentUser';

import { AttendanceOverviewCard } from './components/AttendanceOverviewCard';
import { DepartmentBreakdownCard } from './components/DepartmentBreakdownCard';
import { QuickActionsCard } from './components/QuickActionsCard';
import { RecentActivityCard } from './components/RecentActivityCard';
import { TeamStatusCard } from './components/TeamStatusCard';
import { UpcomingEventsCard } from './components/UpcomingEventsCard';
import { dashboardStats } from './dashboardData';

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

      <div className='grid gap-4 sm:grid-cols-2 xl:grid-cols-4'>
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
      </div>

      <div className='grid gap-4 lg:grid-cols-3'>
        <AttendanceOverviewCard className='lg:col-span-2' />
        <DepartmentBreakdownCard />
      </div>

      <div className='grid gap-4 lg:grid-cols-3'>
        <TeamStatusCard className='lg:col-span-2' />
        <UpcomingEventsCard />
      </div>

      <div className='grid gap-4 lg:grid-cols-3'>
        <RecentActivityCard className='lg:col-span-2' />
        <QuickActionsCard />
      </div>
    </div>
  );
}
