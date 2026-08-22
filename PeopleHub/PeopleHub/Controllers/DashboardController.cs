using Microsoft.AspNetCore.Mvc;
using PeopleHub.Models;
using PeopleHub.Services;

namespace PeopleHub.Controllers
{
    // Base route: /api/dashboard
    // Separate from EmployeesController on purpose: the dashboard aggregates
    // numbers across domains (employees, attendance, requests), while
    // EmployeesController stays focused on plain employee CRUD.
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAttendanceService _attendanceService;
        private readonly IRequestService _requestService;

        public DashboardController(
            IEmployeeService employeeService,
            IAttendanceService attendanceService,
            IRequestService requestService)
        {
            _employeeService = employeeService;
            _attendanceService = attendanceService;
            _requestService = requestService;
        }

        // GET /api/dashboard/summary
        // Everything the four top cards need, in one round trip.
        // Each TrendStat carries { current, previous, changePercent } — use the
        // percentage as-is, or recompute it from the two raw numbers.
        [HttpGet("summary")]
        public ActionResult<DashboardSummary> GetSummary()
        {
            return Ok(new DashboardSummary(
                Headcount: _employeeService.GetHeadcountTrend(),
                AttendanceRate: _attendanceService.GetMonthlyRateTrend(DateTime.Today),
                Leave: new LeaveCard(
                    _requestService.GetOnLeaveCount(DateTime.Today),
                    _requestService.GetActiveLeaveRequestCount()),
                YearlyPayroll: _employeeService.GetPayrollTrend(),
                GeneratedAt: DateTime.Now));
        }

        // GET /api/dashboard/hiring-by-year
        // Raw counts — e.g. [{ year: 2024, count: 18 }, { year: 2025, count: 22 }].
        [HttpGet("hiring-by-year")]
        public IActionResult GetHiringByYear()
        {
            var byYear = _employeeService.GetAll()
                .GroupBy(e => e.HireDate.Year)
                .OrderBy(g => g.Key)
                .Select(g => new { year = g.Key, count = g.Count() });

            return Ok(byYear);
        }

        // GET /api/dashboard/headcount-by-department
        [HttpGet("headcount-by-department")]
        public ActionResult<List<DepartmentHeadcount>> GetHeadcountByDepartment() =>
            Ok(_employeeService.GetHeadcountByDepartment());

        // GET /api/dashboard/attendance-trend?days=7
        // Working days only — Fridays and Saturdays simply have no point.
        [HttpGet("attendance-trend")]
        public ActionResult<List<DailyAttendancePoint>> GetAttendanceTrend([FromQuery] int days = 7) =>
            Ok(_attendanceService.GetTrend(days));

        // GET /api/dashboard/team-status
        // Who is in the office / remote / on leave / absent. On a weekend this
        // reports the most recent working day; the response says which.
        [HttpGet("team-status")]
        public ActionResult<TeamStatusToday> GetTeamStatus() =>
            Ok(_attendanceService.GetTodayStatus());

        // GET /api/dashboard/recent-activity?take=8
        [HttpGet("recent-activity")]
        public ActionResult<List<ActivityItem>> GetRecentActivity([FromQuery] int take = 8) =>
            Ok(_requestService.GetRecentActivity(take));
    }
}
