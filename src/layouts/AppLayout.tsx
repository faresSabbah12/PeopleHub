import { useTranslation } from 'react-i18next';

import { SidebarProvider } from '@/components/ui/sidebar';

import { AppHeader } from './AppHeader';
import { AppSidebar } from './AppSidebar';

export function AppLayout() {
  const { t } = useTranslation();

  return (
    <SidebarProvider>
      <AppSidebar />

      <div className='flex min-h-screen flex-1 flex-col'>
        <AppHeader />

        <main className='flex-1 bg-background p-6'>{t('MAIN_CONTENT')}</main>
      </div>
    </SidebarProvider>
  );
}
