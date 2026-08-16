import type { ReactNode } from 'react';

import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { cn } from '@/lib/utils';

interface SectionCardProps {
  title: string;
  description?: string;
  /** Header-level action, e.g. a "view all" button. */
  action?: ReactNode;
  children: ReactNode;
  /** Removes content padding, for edge-to-edge tables and lists. */
  flush?: boolean;
  className?: string;
  contentClassName?: string;
}

/** Titled content container used by every module section. */
export function SectionCard({
  title,
  description,
  action,
  children,
  flush,
  className,
  contentClassName,
}: SectionCardProps) {
  return (
    <Card className={cn('gap-4', className)}>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        {description && <CardDescription>{description}</CardDescription>}
        {action && <CardAction>{action}</CardAction>}
      </CardHeader>

      <CardContent className={cn(flush && 'px-0', contentClassName)}>
        {children}
      </CardContent>
    </Card>
  );
}
