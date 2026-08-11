import { Bell } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { LanguageSwitcher } from '@/components/language/LanguageSwitcher';
import { SearchInput } from '@/components/common/SearchInput';
import { UserSummary } from '@/components/common/UserSummary';
import { ThemeToggle } from '@/components/theme/ThemeToggle';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { SidebarTrigger } from '@/components/ui/sidebar';
import { currentUser } from '@/data/currentUser';

interface AppHeaderProps {
  /** Already-translated label of the current module. */
  title: string;
}

export function AppHeader({ title }: AppHeaderProps) {
  const { t } = useTranslation();

  return (
    <header className='sticky top-0 z-20 flex h-14 shrink-0 items-center gap-2 border-b border-border bg-background px-3 md:px-4'>
      {/* The panel icon mirrors with the writing direction. */}
      <SidebarTrigger
        className='shrink-0 rtl:[&_svg]:-scale-x-100'
        aria-label={t('TOGGLE_SIDEBAR')}
      />

      <Separator orientation='vertical' className='mx-1 h-5' />

      <p className='truncate text-sm font-medium'>{title}</p>

      <div className='ms-auto flex items-center gap-1.5'>
        <SearchInput
          placeholder={t('SEARCH_PLACEHOLDER')}
          aria-label={t('SEARCH_PLACEHOLDER')}
          className='hidden w-56 lg:block'
        />

        <Button
          variant='ghost'
          size='icon'
          aria-label={t('NOTIFICATIONS')}
          className='relative'
        >
          <Bell />
          <span className='absolute inset-e-1.5 top-1.5 size-1.5 rounded-full bg-primary' />
        </Button>

        <LanguageSwitcher />
        <ThemeToggle />

        <Separator orientation='vertical' className='mx-1 h-5' />

        <UserSummary name={currentUser.name} compact size='sm' />
      </div>
    </header>
  );
}
