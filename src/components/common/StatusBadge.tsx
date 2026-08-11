import { Badge } from '@/components/ui/badge';
import type { Status } from '@/lib/status';
import { cn } from '@/lib/utils';

const statusVariants = {
  present: 'success',
  remote: 'info',
  leave: 'warning',
  absent: 'destructive',
  pending: 'warning',
  approved: 'success',
  rejected: 'destructive',
} as const satisfies Record<
  Status,
  'success' | 'info' | 'warning' | 'destructive'
>;

const dotColors: Record<Status, string> = {
  present: 'bg-success',
  remote: 'bg-info',
  leave: 'bg-warning',
  absent: 'bg-destructive',
  pending: 'bg-warning',
  approved: 'bg-success',
  rejected: 'bg-destructive',
};

interface StatusBadgeProps {
  status: Status;
  /** Already-translated label, e.g. t(statusLabelKeys[status]). */
  label: string;
  className?: string;
}

export function StatusBadge({ status, label, className }: StatusBadgeProps) {
  return (
    <Badge
      variant={statusVariants[status]}
      className={cn('gap-1.5', className)}
    >
      <span
        className={cn('size-1.5 shrink-0 rounded-full', dotColors[status])}
        aria-hidden
      />
      {label}
    </Badge>
  );
}
