import {
  CalendarClock,
  CalendarPlus,
  FileSpreadsheet,
  UserPlus,
  UsersRound,
  Wallet,
  type LucideIcon,
} from 'lucide-react';

import type { Status } from '@/lib/status';
import type { StatTone } from '@/components/common/StatCard';

/**
 * Static dashboard data. Labels are translation keys from the `dashboard`
 * namespace so the shape can be swapped for API responses without touching UI.
 */

export interface DashboardStat {
  id: string;
  labelKey: string;
  value: string;
  icon: LucideIcon;
  tone: StatTone;
  trend?: { value: string; direction: 'up' | 'down' };
  hintKey?: string;
}

export const dashboardStats: DashboardStat[] = [
  {
    id: 'headcount',
    labelKey: 'STAT_TOTAL_EMPLOYEES',
    value: '248',
    icon: UsersRound,
    tone: 'brand',
    trend: { value: '+3.2%', direction: 'up' },
    hintKey: 'STAT_VS_LAST_MONTH',
  },
  {
    id: 'present',
    labelKey: 'STAT_PRESENT_TODAY',
    value: '211',
    icon: CalendarClock,
    tone: 'success',
    trend: { value: '85%', direction: 'up' },
    hintKey: 'STAT_ATTENDANCE_RATE',
  },
  {
    id: 'leave',
    labelKey: 'STAT_ON_LEAVE',
    value: '14',
    icon: CalendarPlus,
    tone: 'warning',
    hintKey: 'STAT_PENDING_REQUESTS',
  },
  {
    id: 'payroll',
    labelKey: 'STAT_MONTHLY_PAYROLL',
    value: '$412.8K',
    icon: Wallet,
    tone: 'info',
    trend: { value: '+1.4%', direction: 'up' },
    hintKey: 'STAT_VS_LAST_MONTH',
  },
];

export interface AttendanceDay {
  dayKey: string;
  presentRate: number;
}

export const attendanceWeek: AttendanceDay[] = [
  { dayKey: 'DAY_MON', presentRate: 88 },
  { dayKey: 'DAY_TUE', presentRate: 92 },
  { dayKey: 'DAY_WED', presentRate: 85 },
  { dayKey: 'DAY_THU', presentRate: 79 },
  { dayKey: 'DAY_FRI', presentRate: 71 },
  { dayKey: 'DAY_SAT', presentRate: 34 },
  { dayKey: 'DAY_SUN', presentRate: 12 },
];

export interface AttendanceBreakdownItem {
  status: Status;
  count: number;
}

export const attendanceBreakdown: AttendanceBreakdownItem[] = [
  { status: 'present', count: 187 },
  { status: 'remote', count: 24 },
  { status: 'leave', count: 14 },
  { status: 'absent', count: 23 },
];

export interface Department {
  id: string;
  nameKey: string;
  headcount: number;
  /** Share of total headcount, already computed by the backend. */
  share: number;
}

export const departments: Department[] = [
  { id: 'eng', nameKey: 'DEPT_ENGINEERING', headcount: 86, share: 35 },
  { id: 'sales', nameKey: 'DEPT_SALES', headcount: 52, share: 21 },
  { id: 'support', nameKey: 'DEPT_SUPPORT', headcount: 41, share: 17 },
  { id: 'finance', nameKey: 'DEPT_FINANCE', headcount: 34, share: 14 },
  { id: 'people', nameKey: 'DEPT_PEOPLE', headcount: 35, share: 13 },
];

export interface TeamMemberRow {
  id: string;
  name: string;
  roleKey: string;
  departmentKey: string;
  status: Status;
  checkIn: string;
}

export const teamStatus: TeamMemberRow[] = [
  {
    id: '1',
    name: 'Layla Haddad',
    roleKey: 'ROLE_PRODUCT_DESIGNER',
    departmentKey: 'DEPT_ENGINEERING',
    status: 'present',
    checkIn: '08:54',
  },
  {
    id: '2',
    name: 'Omar Khalil',
    roleKey: 'ROLE_BACKEND_ENGINEER',
    departmentKey: 'DEPT_ENGINEERING',
    status: 'remote',
    checkIn: '09:10',
  },
  {
    id: '3',
    name: 'Sara Mansour',
    roleKey: 'ROLE_ACCOUNT_MANAGER',
    departmentKey: 'DEPT_SALES',
    status: 'leave',
    checkIn: '—',
  },
  {
    id: '4',
    name: 'Yousef Nabil',
    roleKey: 'ROLE_SUPPORT_SPECIALIST',
    departmentKey: 'DEPT_SUPPORT',
    status: 'present',
    checkIn: '08:31',
  },
  {
    id: '5',
    name: 'Nour Sabri',
    roleKey: 'ROLE_PAYROLL_OFFICER',
    departmentKey: 'DEPT_FINANCE',
    status: 'absent',
    checkIn: '—',
  },
];

export interface ActivityItem {
  id: string;
  actor: string;
  messageKey: string;
  status: Status;
  minutesAgo: number;
}

export const recentActivity: ActivityItem[] = [
  {
    id: '1',
    actor: 'Sara Mansour',
    messageKey: 'ACTIVITY_LEAVE_REQUEST',
    status: 'pending',
    minutesAgo: 25,
  },
  {
    id: '2',
    actor: 'Omar Khalil',
    messageKey: 'ACTIVITY_TIMESHEET_SUBMITTED',
    status: 'approved',
    minutesAgo: 96,
  },
  {
    id: '3',
    actor: 'Hana Aziz',
    messageKey: 'ACTIVITY_CONTRACT_SIGNED',
    status: 'approved',
    minutesAgo: 240,
  },
  {
    id: '4',
    actor: 'Yousef Nabil',
    messageKey: 'ACTIVITY_EXPENSE_REJECTED',
    status: 'rejected',
    minutesAgo: 420,
  },
];

export interface HrEvent {
  id: string;
  titleKey: string;
  /** ISO date, formatted per active locale at render time. */
  date: string;
  countdownDays: number;
}

export const upcomingEvents: HrEvent[] = [
  {
    id: '1',
    titleKey: 'EVENT_PAYROLL_CUTOFF',
    date: '2026-08-25',
    countdownDays: 14,
  },
  {
    id: '2',
    titleKey: 'EVENT_PERFORMANCE_REVIEWS',
    date: '2026-09-01',
    countdownDays: 21,
  },
  {
    id: '3',
    titleKey: 'EVENT_ONBOARDING_SESSION',
    date: '2026-08-18',
    countdownDays: 7,
  },
];

export interface QuickAction {
  id: string;
  labelKey: string;
  icon: LucideIcon;
}

export const quickActions: QuickAction[] = [
  { id: 'add-employee', labelKey: 'ACTION_ADD_EMPLOYEE', icon: UserPlus },
  { id: 'run-payroll', labelKey: 'ACTION_RUN_PAYROLL', icon: Wallet },
  {
    id: 'schedule-leave',
    labelKey: 'ACTION_SCHEDULE_LEAVE',
    icon: CalendarPlus,
  },
  {
    id: 'export-report',
    labelKey: 'ACTION_EXPORT_REPORT',
    icon: FileSpreadsheet,
  },
];
