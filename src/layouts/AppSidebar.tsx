import { useTranslation } from 'react-i18next';

import { PeopleHubLogo } from '@/components/brand/PeopleHubLogo';
import { UserSummary } from '@/components/common/UserSummary';
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from '@/components/ui/sidebar';
import { useSidebar } from '@/components/ui/sidebarContext';
import { currentUser } from '@/data/currentUser';
import { getDirection, type Language } from '@/i18n/language';
import { appRoutes } from '@/routes/routes';

interface AppSidebarProps {
  activePath: string;
  onNavigate: (path: string) => void;
}

export function AppSidebar({ activePath, onNavigate }: AppSidebarProps) {
  const sidebarRoutes = appRoutes.filter((route) => route.showInSidebar);
  const { t, i18n } = useTranslation(['sideMenu', 'common']);
  const { isMobile, setOpenMobile } = useSidebar();
  const isRtl = getDirection(i18n.language as Language) === 'rtl';

  const handleNavigate = (path: string) => {
    onNavigate(path);

    if (isMobile) {
      setOpenMobile(false);
    }
  };

  return (
    <Sidebar side={isRtl ? 'right' : 'left'} collapsible='icon'>
      <SidebarHeader className='h-14 justify-center border-b border-sidebar-border px-3 group-data-[collapsible=icon]:px-1.5'>
        <PeopleHubLogo
          markOnly={false}
          className='group-data-[collapsible=icon]:[&>span:last-child]:hidden'
        />
      </SidebarHeader>

      <SidebarContent className='px-1.5 py-3'>
        <SidebarGroup className='p-0'>
          <SidebarGroupLabel>{t('WORKSPACE')}</SidebarGroupLabel>

          <SidebarGroupContent>
            <SidebarMenu>
              {sidebarRoutes.map((route) => {
                const Icon = route.icon;
                const label = t(route.labelKey);

                return (
                  <SidebarMenuItem key={route.path}>
                    <SidebarMenuButton
                      isActive={activePath === route.path}
                      tooltip={label}
                      onClick={() => handleNavigate(route.path)}
                    >
                      {Icon && <Icon />}
                      <span>{label}</span>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter className='border-t border-sidebar-border p-2'>
        <UserSummary
          name={currentUser.name}
          meta={t(currentUser.roleKey)}
          compact={false}
          className='px-1 group-data-[collapsible=icon]:[&>div]:hidden'
        />
      </SidebarFooter>

      <SidebarRail />
    </Sidebar>
  );
}
