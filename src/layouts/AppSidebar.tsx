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
import { appRoutes } from '@/routes/routes';

export function AppSidebar() {
  const sidebarRoutes = appRoutes.filter((rout) => rout.showInSidebar === true);

  return (
    <Sidebar>
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
                      <span>{route.label}</span>
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
