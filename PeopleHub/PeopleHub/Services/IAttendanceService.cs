using PeopleHub.Models;

namespace PeopleHub.Services
{
    public interface IAttendanceService
    {
        List<AttendanceRecord> Filter(AttendanceQuery query);
        PagedResult<AttendanceRecord> Query(AttendanceQuery query);

        // Aggregates for whatever the same filters matched (ignores paging).
        AttendanceSummary GetSummary(AttendanceQuery query);

        // Dashboard card: this month-to-date attendance rate vs all of last month.
        TrendStat GetMonthlyRateTrend(DateTime asOf);

        // Dashboard chart: one point per working day over the last N days.
        List<DailyAttendancePoint> GetTrend(int days);

        // Dashboard card: who is in / remote / on leave / absent right now.
        TeamStatusToday GetTodayStatus();

        int RecordCount { get; }
    }
}
