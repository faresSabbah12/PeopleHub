import {
  Clock3,
  LayoutDashboard,
  Settings,
  Users,
  Wallet,
  type LucideIcon,
} from 'lucide-react';

export interface AppRoute {
  path: string;
  labelKey: string;
  icon?: LucideIcon;
  showInSidebar?: boolean;
  children?: AppRoute[];
}

export const appRoutes: AppRoute[] = [
  {
    path: '/dashboard',
    labelKey: 'DASHBOARD',
    icon: LayoutDashboard,
    showInSidebar: true,
  },
  {
    path: '/employees',
    labelKey: 'EMPLOYEES',
    icon: Users,
    showInSidebar: true,
  },
  {
    path: '/salaries',
    labelKey: 'SALARIES',
    icon: Wallet,
    showInSidebar: true,
  },
  {
    path: '/attendance',
    labelKey: 'ATTENDANCE',
    icon: Clock3,
    showInSidebar: true,
  },
  {
    path: '/settings',
    labelKey: 'SETTINGS',
    icon: Settings,
    showInSidebar: true,
  },
];
