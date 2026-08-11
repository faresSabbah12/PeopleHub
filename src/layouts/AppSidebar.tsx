import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from '@/components/ui/sidebar';
import { useTranslation } from 'react-i18next';
import { appRoutes } from '@/routes/routes';

export function AppSidebar() {
  const sidebarRoutes = appRoutes.filter((rout) => rout.showInSidebar === true);
  const { t, i18n } = useTranslation('sideMenu');
  const isArabic = i18n.language === 'ar';

  return (
    <Sidebar side={isArabic ? 'right' : 'left'}>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>PeopleHub</SidebarGroupLabel>

          <SidebarGroupContent>
            <SidebarMenu>
              {sidebarRoutes.map((route) => {
                const Icon = route.icon;

                return (
                  <SidebarMenuItem key={route.path}>
                    <SidebarMenuButton>
                      {Icon && <Icon />}
                      <span>{t(route.labelKey)}</span>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  );
}
