'use client';

import * as React from 'react';
import { Progress as ProgressPrimitive } from 'radix-ui';

import { cn } from '@/lib/utils';

function Progress({
  className,
  value,
  ...props
}: React.ComponentProps<typeof ProgressPrimitive.Root>) {
  return (
    <ProgressPrimitive.Root
      data-slot='progress'
      className={cn(
        'relative flex h-1.5 w-full items-center overflow-hidden rounded-full bg-muted',
        className,
      )}
      {...props}
    >
      {/* Width (not translateX) so the bar fills from the inline start in RTL too. */}
      <ProgressPrimitive.Indicator
        data-slot='progress-indicator'
        className='h-full rounded-full bg-primary transition-[width]'
        style={{ width: `${value ?? 0}%` }}
      />
    </ProgressPrimitive.Root>
  );
}

export { Progress };
