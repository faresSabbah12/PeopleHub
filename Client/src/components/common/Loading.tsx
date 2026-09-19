import { PeopleHubLogo } from '@/components/brand/PeopleHubLogo';
import { cn } from '@/lib/utils';

const SPINNER_SIZES = {
  sm: 'size-4 border-2',
  md: 'size-6 border-2',
  lg: 'size-10 border-[3px]',
} as const;

interface SpinnerProps {
  size?: keyof typeof SPINNER_SIZES;
  className?: string;
}

/** Brand-colored ring spinner for inline, button and card loading states. */
export function Spinner({ size = 'md', className }: SpinnerProps) {
  return (
    <span
      role='status'
      aria-label='Loading'
      className={cn(
        'inline-block animate-spin rounded-full border-primary/20 border-r-primary/60 border-t-primary',
        SPINNER_SIZES[size],
        className,
      )}
    />
  );
}

interface PageLoaderProps {
  message?: string;
  className?: string;
}

/** Full-page/section loading state: pulsing brand mark + spinner. Use for route and Suspense fallbacks. */
export function PageLoader({ message, className }: PageLoaderProps) {
  return (
    <div
      className={cn(
        'flex min-h-[60vh] flex-col items-center justify-center gap-4',
        className,
      )}
    >
      <div className='animate-pulse'>
        <PeopleHubLogo markOnly />
      </div>
      <Spinner size='lg' />
      {message && <p className='text-sm text-muted-foreground'>{message}</p>}
    </div>
  );
}
