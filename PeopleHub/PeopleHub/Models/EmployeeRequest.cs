namespace PeopleHub.Models
{
    public static class RequestStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";

        public static readonly string[] All = { Pending, Approved, Rejected };

        public static bool IsValid(string? value) =>
            All.Any(s => string.Equals(s, value, StringComparison.OrdinalIgnoreCase));

        // Normalises "approved" / "APPROVED" to the canonical "Approved".
        public static string? Normalise(string? value) =>
            All.FirstOrDefault(s => string.Equals(s, value, StringComparison.OrdinalIgnoreCase));
    }

    // A request an employee submits to HR: time off, remote work, overtime, a
    // salary advance, or a resignation. Same denormalisation as
    // AttendanceRecord — the employee's display fields are copied on.
    public class EmployeeRequest
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;

        // One of Data/RequestCatalog.Types
        public string Type { get; set; } = string.Empty;

        // Only set when Type == "Leave" (Annual / Sick / Unpaid / ...).
        // The `?` means "this is allowed to be null" — see BACKEND_GUIDE.md.
        public string? LeaveType { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Inclusive of both ends: a 1-day leave has Days == 1.
        public int Days => (EndDate.Date - StartDate.Date).Days + 1;

        public string Status { get; set; } = RequestStatus.Pending;
        public string Reason { get; set; } = string.Empty;

        // Only meaningful for Loan requests (in JD).
        public decimal? Amount { get; set; }

        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
    }
}
