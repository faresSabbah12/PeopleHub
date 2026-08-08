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
  label: string;
  icon?: LucideIcon;
  showInSidebar?: boolean;
  children?: AppRoute[];
}

export const appRoutes: AppRoute[] = [
  {
    path: '/dashboard',
    label: 'DASHBOARD',
    icon: LayoutDashboard,
    showInSidebar: true,
  },
  {
    path: '/employees',
    label: 'EMPLOYEES',
    icon: Users,
    showInSidebar: true,
  },
  {
    path: '/salaries',
    label: 'SALARIES',
    icon: Wallet,
    showInSidebar: true,
  },
  {
    path: '/attendance',
    label: 'ATTENDANCE',
    icon: Clock3,
    showInSidebar: true,
  },
  {
    path: '/settings',
    label: 'SETTINGS',
    icon: Settings,
    showInSidebar: true,
  },
];
