import { useState } from 'react';
import { useTranslation } from 'react-i18next';

import { SidebarInset, SidebarProvider } from '@/components/ui/sidebar';
import { TooltipProvider } from '@/components/ui/tooltip';
import { DashboardPage } from '@/pages/dashboard/DashboardPage';
import { ModulePlaceholderPage } from '@/pages/ModulePlaceholderPage';
import { appRoutes } from '@/routes/routes';

import { AppHeader } from './AppHeader';
import { AppSidebar } from './AppSidebar';

const defaultPath = appRoutes[0].path;

export function AppLayout() {
  // Local navigation state until a router is introduced; the shell only needs
  // to know which module is on screen.
  const [activePath, setActivePath] = useState(defaultPath);
  const { t } = useTranslation('sideMenu');

  const activeRoute =
    appRoutes.find((route) => route.path === activePath) ?? appRoutes[0];
  const activeTitle = t(activeRoute.labelKey);

  return (
    <TooltipProvider delayDuration={200}>
      <SidebarProvider>
        <AppSidebar activePath={activePath} onNavigate={setActivePath} />

        <SidebarInset className='flex min-w-0 flex-col'>
          <AppHeader title={activeTitle} />

          <div className='flex-1 px-4 py-5 md:px-6 md:py-6'>
            {activeRoute.path === defaultPath ? (
              <DashboardPage />
            ) : (
              <ModulePlaceholderPage
                title={activeTitle}
                icon={activeRoute.icon}
              />
            )}
          </div>
        </SidebarInset>
      </SidebarProvider>
    </TooltipProvider>
  );
}
