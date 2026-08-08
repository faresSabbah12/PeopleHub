import { SidebarTrigger } from '@/components/ui/sidebar';
import { ThemeToggle } from '@/components/theme/ThemeToggle';

export function AppHeader() {
  return (
    <header className='flex h-14 items-center justify-between border-b px-4'>
      <SidebarTrigger />

      <div className='flex items-center gap-2'>
        <ThemeToggle />
      </div>
    </header>
  );
}
