import { useTranslation } from 'react-i18next';

import { SectionCard } from '@/components/common/SectionCard';
import { StatusBadge } from '@/components/common/StatusBadge';
import { statusLabelKeys } from '@/lib/status';
import { UserSummary } from '@/components/common/UserSummary';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { teamStatus } from '../dashboardData';

/** Today's roster — also the reference styling for module tables. */
export function TeamStatusCard({ className }: { className?: string }) {
  const { t } = useTranslation('dashboard');

  return (
    <SectionCard
      className={className}
      title={t('TEAM_STATUS')}
      description={t('TEAM_STATUS_DESCRIPTION')}
      action={
        <Button variant='ghost' size='sm'>
          {t('VIEW_ALL')}
        </Button>
      }
      flush
    >
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className='ps-5'>{t('TABLE_EMPLOYEE')}</TableHead>
            <TableHead className='hidden sm:table-cell'>
              {t('TABLE_DEPARTMENT')}
            </TableHead>
            <TableHead>{t('TABLE_STATUS')}</TableHead>
            <TableHead className='pe-5 text-end'>
              {t('TABLE_CHECK_IN')}
            </TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {teamStatus.map((member) => (
            <TableRow key={member.id} className='last:border-0'>
              <TableCell className='ps-5'>
                <UserSummary
                  name={member.name}
                  meta={t(member.roleKey)}
                  size='sm'
                />
              </TableCell>

              <TableCell className='hidden text-muted-foreground sm:table-cell'>
                {t(member.departmentKey)}
              </TableCell>

              <TableCell>
                <StatusBadge
                  status={member.status}
                  label={t(statusLabelKeys[member.status])}
                />
              </TableCell>

              <TableCell className='pe-5 text-end tabular-nums'>
                {member.checkIn}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </SectionCard>
  );
}
