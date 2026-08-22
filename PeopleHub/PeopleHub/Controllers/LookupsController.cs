using Microsoft.AspNetCore.Mvc;
using PeopleHub.Data;
using PeopleHub.Models;

namespace PeopleHub.Controllers
{
    // Shared reference data other pages/forms pull from (dropdowns, filters...).
    // Every list here is read from the SAME statics used to seed the mock data,
    // so a filter dropdown can never offer a value that matches nothing.
    [ApiController]
    [Route("api/[controller]")]
    public class LookupsController : ControllerBase
    {
        [HttpGet("departments")]
        public ActionResult<List<LookupItem>> GetDepartments() => Ok(ToLookups(Departments.All));

        // GET /api/lookups/job-titles?department=Engineering
        // Without the parameter: every title in the company, de-duplicated.
        [HttpGet("job-titles")]
        public ActionResult<List<LookupItem>> GetJobTitles([FromQuery] string? department)
        {
            var titles = string.IsNullOrWhiteSpace(department)
                ? Departments.JobTitlesByDepartment.Values.SelectMany(t => t).Distinct().OrderBy(t => t)
                : Departments.JobTitlesByDepartment
                    .Where(kv => kv.Key.Equals(department, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(kv => kv.Value);

            return Ok(ToLookups(titles));
        }

        // Departments and their titles together — one call to populate a
        // dependent "department -> job title" pair of dropdowns.
        [HttpGet("departments-with-titles")]
        public IActionResult GetDepartmentsWithTitles() =>
            Ok(Departments.JobTitlesByDepartment.Select(kv => new { department = kv.Key, jobTitles = kv.Value }));

        [HttpGet("request-types")]
        public ActionResult<List<LookupItem>> GetRequestTypes() => Ok(ToLookups(RequestCatalog.Types));

        [HttpGet("leave-types")]
        public ActionResult<List<LookupItem>> GetLeaveTypes() => Ok(ToLookups(RequestCatalog.LeaveTypes));

        [HttpGet("request-statuses")]
        public ActionResult<List<LookupItem>> GetRequestStatuses() => Ok(ToLookups(RequestStatus.All));

        [HttpGet("attendance-statuses")]
        public ActionResult<List<LookupItem>> GetAttendanceStatuses() => Ok(ToLookups(AttendanceStatus.All));

        // GET /api/lookups/work-hours — office hours the Attendance page labels
        // its "late / early" filters with, rather than hardcoding 09:00-17:00.
        [HttpGet("work-hours")]
        public IActionResult GetWorkHours() =>
            Ok(new
            {
                start = AttendanceRecord.WorkStart,
                end = AttendanceRecord.WorkEnd,
                lateGraceMinutes = AttendanceRecord.LateGraceMinutes,
                earlyLeaveGraceMinutes = AttendanceRecord.EarlyLeaveGraceMinutes,
                workdays = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" }
            });

        // Turns any list of strings into the {id, name} shape every dropdown
        // already expects. The id is just the position — these are not entities.
        private static List<LookupItem> ToLookups(IEnumerable<string> values) =>
            values.Select((name, index) => new LookupItem(index + 1, name)).ToList();
    }
}
