// Mirrors PeopleHub/Models/DashboardModels.cs — keep in sync with the backend model.
export interface TrendStat {
  current: number;
  previous: number;
  changePercent: number;
}

export interface LeaveCard {
  onLeaveToday: number;
  activeLeaveRequests: number;
}

export interface DashboardSummary {
  headcount: TrendStat;
  attendanceRate: TrendStat;
  leave: LeaveCard;
  yearlyPayroll: TrendStat;
  generatedAt: string;
}
