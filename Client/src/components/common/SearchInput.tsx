import type { ComponentProps } from 'react';
import { Search } from 'lucide-react';

import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';

type SearchInputProps = Omit<ComponentProps<'input'>, 'type'>;

/** Search field with a leading icon that follows the writing direction. */
export function SearchInput({ className, ...props }: SearchInputProps) {
  return (
    <div className='relative'>
      <Search
        className='pointer-events-none absolute inset-s-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground'
        aria-hidden
      />

      <Input type='search' className={cn('h-9 ps-8.5', className)} {...props} />
    </div>
  );
}
