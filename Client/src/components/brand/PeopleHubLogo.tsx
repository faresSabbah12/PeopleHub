import { UsersRound } from 'lucide-react';

import { cn } from '@/lib/utils';

interface PeopleHubLogoProps {
  /** Hides the wordmark, e.g. in the collapsed sidebar. */
  markOnly?: boolean;
  className?: string;
}

/**
 * The brand lockup. The only place besides the active nav item where the
 * brand gradient is allowed.
 */
export function PeopleHubLogo({ markOnly, className }: PeopleHubLogoProps) {
  return (
    <span className={cn('flex items-center gap-2.5', className)}>
      <span className='flex size-8 shrink-0 items-center justify-center rounded-md bg-brand-gradient text-primary-foreground shadow-xs'>
        <UsersRound className='size-4.5' aria-hidden />
      </span>

      {!markOnly && (
        <span className='flex min-w-0 flex-col leading-none'>
          <span className='font-heading truncate text-sm font-semibold tracking-tight'>
            PeopleHub
          </span>
          <span className='mt-1 truncate text-[0.6875rem] text-muted-foreground'>
            HR Suite
          </span>
        </span>
      )}
    </span>
  );
}
