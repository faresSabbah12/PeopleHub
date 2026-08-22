using Microsoft.AspNetCore.Mvc;
using PeopleHub.Models;
using PeopleHub.Services;

namespace PeopleHub.Controllers
{
    // Base route: /api/attendance
    // Backs the Attendance page: one big filterable table plus a summary strip.
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET /api/attendance
        //   ?keyword=          free text over name, code, department, job title, status
        //   &name=             the dedicated "search by name" field
        //   &employeeCode=     the dedicated "search by id" field (partial match)
        //   &employeeId=       exact numeric id
        //   &department=&status=
        //   &date=2026-08-19            one exact day (overrides from/to)
        //   &from=2026-08-01&to=2026-08-19
        //   &sort=latestCheckIn | earliestCheckIn | date | name | lateMinutes | workedHours | status
        //   &sortDir=asc|desc&page=1&pageSize=25
        //
        // Office hours are 09:00-17:00. latestCheckIn / earliestCheckIn already
        // carry their own direction, so sortDir is ignored for those two.
        // Default order is newest day first.
        [HttpGet]
        public ActionResult<PagedResult<AttendanceRecord>> Get([FromQuery] AttendanceQuery query) =>
            Ok(_attendanceService.Query(query));

        // GET /api/attendance/summary — same filters, aggregated instead of paged.
        // attendanceRate excludes approved leave from both sides of the ratio.
        [HttpGet("summary")]
        public ActionResult<AttendanceSummary> GetSummary([FromQuery] AttendanceQuery query) =>
            Ok(_attendanceService.GetSummary(query));
    }
}
