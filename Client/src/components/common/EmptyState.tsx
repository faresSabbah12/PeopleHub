import type { ReactNode } from 'react';
import type { LucideIcon } from 'lucide-react';

import { cn } from '@/lib/utils';

interface EmptyStateProps {
  icon: LucideIcon;
  title: string;
  description?: string;
  action?: ReactNode;
  className?: string;
}

/** Shared placeholder for empty lists, filtered-out results and unbuilt modules. */
export function EmptyState({
  icon: Icon,
  title,
  description,
  action,
  className,
}: EmptyStateProps) {
  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center gap-3 rounded-xl border border-dashed border-border bg-card/40 px-6 py-14 text-center',
        className,
      )}
    >
      <span className='flex size-11 items-center justify-center rounded-full bg-muted text-muted-foreground'>
        <Icon className='size-5' aria-hidden />
      </span>

      <div className='space-y-1'>
        <p className='font-medium'>{title}</p>

        {description && (
          <p className='mx-auto max-w-sm text-sm text-muted-foreground'>
            {description}
          </p>
        )}
      </div>

      {action}
    </div>
  );
}
