namespace PeopleHub.Models
{
    // Statuses are plain strings (not a C# enum) so they serialise to readable
    // JSON — "Late", not 3 — and match what the UI filters send back.
    public static class AttendanceStatus
    {
        public const string Present = "Present";
        public const string Late = "Late";
        public const string EarlyLeave = "EarlyLeave";
        public const string Absent = "Absent";
        public const string OnLeave = "OnLeave";
        public const string Remote = "Remote";

        public static readonly string[] All = { Present, Late, EarlyLeave, Absent, OnLeave, Remote };
    }

    // One row = one employee on one working day.
    //
    // The employee's name/department/etc. are COPIED onto the record instead of
    // just keeping EmployeeId. That's denormalisation: it means the attendance
    // table endpoint can return everything the UI needs in one flat object with
    // no second lookup, and keyword search can hit the name directly.
    public class AttendanceRecord
    {
        // Official office hours. Used to seed the data AND to derive
        // late/early-leave minutes, so both always agree.
        public static readonly TimeSpan WorkStart = new(9, 0, 0);   // 09:00
        public static readonly TimeSpan WorkEnd = new(17, 0, 0);    // 17:00

        // Someone at 09:03 is not "late" on any real HR system.
        public const int LateGraceMinutes = 5;
        public const int EarlyLeaveGraceMinutes = 15;

        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        // Null when the person never showed up (Absent / OnLeave).
        // TimeSpan serialises to JSON as "09:14:00".
        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }

        public string Status { get; set; } = AttendanceStatus.Present;

        // Computed on read (no setter) so they can never drift out of sync with
        // CheckIn/CheckOut. They still appear in the JSON.
        public double WorkedHours => CheckIn.HasValue && CheckOut.HasValue
            ? Math.Round((CheckOut.Value - CheckIn.Value).TotalHours, 2)
            : 0;

        public int LateMinutes => CheckIn.HasValue && CheckIn.Value > WorkStart
            ? (int)(CheckIn.Value - WorkStart).TotalMinutes
            : 0;

        public int EarlyLeaveMinutes => CheckOut.HasValue && CheckOut.Value < WorkEnd
            ? (int)(WorkEnd - CheckOut.Value).TotalMinutes
            : 0;
    }
}
