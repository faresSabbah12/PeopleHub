namespace PeopleHub.Models
{
    // Every "X now vs X before, and the % arrow under the card" stat on the
    // dashboard uses this one shape. The percentage is computed server-side in
    // ONE place (From below) so no card can disagree with another — and the raw
    // Current/Previous are still there if you'd rather compute it in React.
    public record TrendStat(decimal Current, decimal Previous, double ChangePercent)
    {
        public static TrendStat From(decimal current, decimal previous)
        {
            // Dividing by a zero baseline is the classic way these cards end up
            // showing "Infinity%" or "NaN%". Handle it once, here.
            double percent = previous == 0
                ? (current == 0 ? 0 : 100)
                : (double)((current - previous) / previous) * 100;

            return new TrendStat(current, previous, Math.Round(percent, 2));
        }
    }

    // ---- Dashboard cards -------------------------------------------------

    public record LeaveCard(int OnLeaveToday, int ActiveLeaveRequests);

    public record DashboardSummary(
        TrendStat Headcount,        // employees now vs one year ago
        TrendStat AttendanceRate,   // this month-to-date vs all of last month, %
        LeaveCard Leave,            // on leave today + pending leave requests
        TrendStat YearlyPayroll,    // this year-to-date vs same period last year
        DateTime GeneratedAt);

    public record DepartmentHeadcount(string Department, int Headcount, double Share);

    public record DailyAttendancePoint(
        DateTime Date, int Present, int Late, int Absent, int OnLeave, int Remote, double AttendanceRate);

    // Date is included because Friday/Saturday have no attendance at all — on a
    // weekend this reports the most recent working day, and says which one.
    public record TeamStatusToday(DateTime Date, int InOffice, int Remote, int OnLeave, int Absent, int Total);

    public record ActivityItem(
        int RequestId, int EmployeeId, string EmployeeName, string AvatarUrl,
        string Type, string Status, DateTime At, string Description);

    // ---- Attendance ------------------------------------------------------

    public record AttendanceSummary(
        int Total, int Present, int Late, int EarlyLeave, int Absent, int OnLeave, int Remote,
        double AttendanceRate,      // attended / scheduled, %  (leave days excluded from both)
        double PunctualityRate,     // on-time / attended, %
        string? AverageCheckIn,     // "09:07", null when nobody checked in
        int TotalLateMinutes);

    // ---- Requests --------------------------------------------------------

    public record RequestSummary(
        int Total,
        int Pending,
        int Approved,
        int Rejected,
        Dictionary<string, int> ByType,
        int OnLeaveToday,
        int ActiveLeaveRequests,
        DateTime From,
        DateTime To);

    // ---- Financial -------------------------------------------------------

    public record SalaryRow(
        int EmployeeId, string EmployeeCode, string EmployeeName, string AvatarUrl,
        string Department, string JobTitle, DateTime HireDate,
        decimal MonthlySalary, decimal AnnualSalary);

    public record SalarySummary(
        int EmployeeCount,
        decimal TotalMonthly,
        decimal TotalAnnual,
        decimal Average,
        decimal Median,
        decimal Min,
        decimal Max);

    public record DepartmentSalary(
        string Department, int Headcount, decimal TotalMonthly, decimal AverageMonthly, double Share);

    public record YearlyPayrollPoint(int Year, decimal Payroll, int Headcount);
}
