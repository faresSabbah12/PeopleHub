import type { ReactNode } from 'react';

import { cn } from '@/lib/utils';

/** Max cards per row at the widest breakpoint. */
const columnsClassMap = {
  2: 'sm:grid-cols-2',
  3: 'lg:grid-cols-3',
  4: 'sm:grid-cols-2 xl:grid-cols-4',
} as const;

interface CardsContainerProps {
  /** How many Cards sit in one row at the widest breakpoint. Defaults to 3. */
  columns?: keyof typeof columnsClassMap;
  children: ReactNode;
  className?: string;
}

/** Grid wrapper for laying out a row of Cards; use a card's className (e.g. `lg:col-span-2`) to widen it within the row. */
export function CardsContainer({
  columns = 3,
  children,
  className,
}: CardsContainerProps) {
  return (
    <div className={cn('grid gap-4', columnsClassMap[columns], className)}>
      {children}
    </div>
  );
}
