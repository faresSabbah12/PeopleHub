import { TrendingDown, TrendingUp, type LucideIcon } from 'lucide-react';

import { Card, CardContent } from '@/components/ui/card';
import { cn } from '@/lib/utils';

export type StatTone = 'brand' | 'success' | 'warning' | 'info';

const toneStyles: Record<StatTone, string> = {
  brand: 'bg-primary-subtle text-primary',
  success: 'bg-success-subtle text-success',
  warning: 'bg-warning-subtle text-warning',
  info: 'bg-info-subtle text-info',
};

interface StatCardProps {
  label: string;
  value: string;
  icon: LucideIcon;
  tone?: StatTone;
  /** Comparison against the previous period. */
  trend?: { value: string; direction: 'up' | 'down' };
  /** Short qualifier under the value, e.g. "of 248 employees". */
  hint?: string;
  className?: string;
}

/** Compact KPI tile for dashboards and module overviews. */
export function StatCard({
  label,
  value,
  icon: Icon,
  tone = 'brand',
  trend,
  hint,
  className,
}: StatCardProps) {
  const TrendIcon = trend?.direction === 'down' ? TrendingDown : TrendingUp;

  return (
    <Card size='sm' className={cn('gap-3', className)}>
      <CardContent className='flex items-start justify-between gap-3'>
        <div className='min-w-0 space-y-1.5'>
          <p className='truncate text-[0.8125rem] font-medium text-muted-foreground'>
            {label}
          </p>

          <p className='font-heading text-2xl font-semibold tracking-tight'>
            {value}
          </p>

          <div className='flex items-center gap-1.5 text-xs'>
            {trend && (
              <span
                className={cn(
                  'inline-flex items-center gap-1 font-medium',
                  trend.direction === 'up'
                    ? 'text-success'
                    : 'text-destructive',
                )}
              >
                <TrendIcon className='size-3.5 rtl:-scale-x-100' aria-hidden />
                {trend.value}
              </span>
            )}

            {hint && (
              <span className='truncate text-muted-foreground'>{hint}</span>
            )}
          </div>
        </div>

        <span
          className={cn(
            'flex size-9 shrink-0 items-center justify-center rounded-md',
            toneStyles[tone],
          )}
        >
          <Icon className='size-4.5' aria-hidden />
        </span>
      </CardContent>
    </Card>
  );
}
