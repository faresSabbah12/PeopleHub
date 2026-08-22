namespace PeopleHub.Models
{
    // These classes are filled in automatically from the URL's query string by
    // ASP.NET Core "model binding" — `?page=2&keyword=ahmad` becomes
    // `Page = 2, Keyword = "ahmad"`. Matching is case-insensitive.
    //
    // Anything the caller omits keeps the default written here. That is why the
    // Requests page gets "this month" for free when it calls /api/requests with
    // no parameters at all.

    public class PagedQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        public string? Sort { get; set; }
        public string? SortDir { get; set; }   // "asc" (default) | "desc"

        public bool Descending =>
            string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase);
    }

    // Used by /api/employees AND /api/financial/* — the salary screens filter
    // the exact same employee list, so they share one query class rather than
    // owning a near-identical copy.
    public class EmployeeQuery : PagedQuery
    {
        public string? Keyword { get; set; }        // name, code, email or phone
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public string? Gender { get; set; }
        public string? MaritalStatus { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public DateTime? HiredFrom { get; set; }
        public DateTime? HiredTo { get; set; }
    }

    public class AttendanceQuery : PagedQuery
    {
        public string? Keyword { get; set; }        // free text: name, code, department, title
        public string? Name { get; set; }           // dedicated "search by name" field
        public string? EmployeeCode { get; set; }   // dedicated "search by id" field
        public int? EmployeeId { get; set; }
        public string? Department { get; set; }
        public string? Status { get; set; }

        public DateTime? Date { get; set; }         // one exact day (wins over From/To)
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class RequestQuery : PagedQuery
    {
        public string? Period { get; set; } = "month";   // day | month | year | all
        public DateTime? Date { get; set; }              // anchor for Period, defaults to today

        public string? Status { get; set; }              // Pending | Approved | Rejected
        public string? Type { get; set; }                // Leave | RemoteWork | ...
        public string? LeaveType { get; set; }
        public string? Department { get; set; }
        public int? EmployeeId { get; set; }
        public string? Keyword { get; set; }             // name, code or reason
    }
}
