import type { ReactNode } from 'react';

import { cn } from '@/lib/utils';

interface PageHeaderProps {
  title: string;
  description?: string;
  /** Primary/secondary page actions, rendered at the inline end. */
  actions?: ReactNode;
  className?: string;
}

/** Standard heading block for every PeopleHub page. */
export function PageHeader({
  title,
  description,
  actions,
  className,
}: PageHeaderProps) {
  return (
    <div
      className={cn(
        'flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between',
        className,
      )}
    >
      <div className='min-w-0 space-y-1'>
        <h1 className='truncate text-xl font-semibold sm:text-2xl'>{title}</h1>

        {description && (
          <p className='text-sm text-muted-foreground'>{description}</p>
        )}
      </div>

      {actions && (
        <div className='flex shrink-0 items-center gap-2'>{actions}</div>
      )}
    </div>
  );
}
