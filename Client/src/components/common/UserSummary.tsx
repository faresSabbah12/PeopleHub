import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { cn } from '@/lib/utils';

interface UserSummaryProps {
  name: string;
  /** Job title, department or any secondary line. */
  meta?: string;
  avatarUrl?: string;
  size?: 'sm' | 'default' | 'lg';
  /** Hides the text block, e.g. in a collapsed sidebar or a tight header. */
  compact?: boolean;
  className?: string;
}

const getInitials = (name: string) =>
  name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase();

/** Person presentation used in tables, headers, activity lists and the sidebar. */
export function UserSummary({
  name,
  meta,
  avatarUrl,
  size = 'default',
  compact,
  className,
}: UserSummaryProps) {
  return (
    <div className={cn('flex min-w-0 items-center gap-2.5', className)}>
      <Avatar size={size}>
        {avatarUrl && <AvatarImage src={avatarUrl} alt='' />}
        <AvatarFallback className='bg-primary-subtle text-primary'>
          {getInitials(name)}
        </AvatarFallback>
      </Avatar>

      {!compact && (
        <div className='min-w-0 leading-tight'>
          <p className='truncate text-sm font-medium'>{name}</p>
          {meta && (
            <p className='truncate text-xs text-muted-foreground'>{meta}</p>
          )}
        </div>
      )}
    </div>
  );
}
