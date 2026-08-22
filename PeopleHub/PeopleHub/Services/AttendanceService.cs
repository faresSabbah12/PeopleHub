using PeopleHub.Data;
using PeopleHub.Helpers;
using PeopleHub.Models;

namespace PeopleHub.Services
{
    // Third mock "table", and by far the biggest: one row per employee per
    // working day for the last two years (~100,000 rows, ~20 MB).
    //
    // It depends on BOTH other services: the employee list gives it the roster,
    // and the request list tells it which days were approved leave — so an
    // approved holiday shows up as "OnLeave", not as an unexplained absence.
    public class AttendanceService : IAttendanceService
    {
        public const int HistoryYears = 2;

        private readonly List<AttendanceRecord> _records;

        public AttendanceService(IEmployeeService employeeService, IRequestService requestService)
        {
            _records = Seed(employeeService, requestService);
        }

        public int RecordCount => _records.Count;

        // ---- Seeding --------------------------------------------------------

        private static List<AttendanceRecord> Seed(IEmployeeService employeeService, IRequestService requestService)
        {
            var random = new Random(1337);
            var today = DateTime.Today;
            var windowStart = today.AddYears(-HistoryYears);

            // Build a lookup of approved date ranges per employee ONCE, instead
            // of re-scanning ~1,400 requests for every one of the ~100,000 days.
            var leaveByEmployee = RangesByEmployee(requestService, RequestCatalog.Leave);
            var remoteByEmployee = RangesByEmployee(requestService, RequestCatalog.RemoteWork);

            var records = new List<AttendanceRecord>();
            int id = 1;

            foreach (var employee in employeeService.GetAll())
            {
                // Attendance starts the later of "two years ago" and "their hire
                // date" — you cannot clock in before you are hired.
                var start = employee.HireDate.Date > windowStart ? employee.HireDate.Date : windowStart;

                leaveByEmployee.TryGetValue(employee.Id, out var leaves);
                remoteByEmployee.TryGetValue(employee.Id, out var remotes);

                for (var day = start; day <= today; day = day.AddDays(1))
                {
                    if (!DateRanges.IsWorkday(day)) continue;   // Fri/Sat weekend

                    string status;
                    TimeSpan? checkIn = null;
                    TimeSpan? checkOut = null;

                    if (Covers(leaves, day))
                    {
                        status = AttendanceStatus.OnLeave;      // approved holiday, no clock-in
                    }
                    else
                    {
                        bool remote = Covers(remotes, day);

                        if (!remote && random.Next(100) < 4)
                        {
                            status = AttendanceStatus.Absent;   // ~4% unexplained
                        }
                        else
                        {
                            checkIn = RandomCheckIn(random);
                            checkOut = RandomCheckOut(random, checkIn.Value);
                            status = remote
                                ? AttendanceStatus.Remote
                                : Classify(checkIn.Value, checkOut.Value);
                        }
                    }

                    records.Add(new AttendanceRecord
                    {
                        Id = id++,
                        EmployeeId = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        EmployeeName = employee.FullName,
                        Department = employee.Department,
                        JobTitle = employee.JobTitle,
                        AvatarUrl = employee.AvatarUrl,
                        Date = day,
                        CheckIn = checkIn,
                        CheckOut = checkOut,
                        Status = status
                    });
                }
            }

            return records;
        }

        private static Dictionary<int, List<(DateTime Start, DateTime End)>> RangesByEmployee(
            IRequestService requestService, string type) =>
            requestService.GetAll()
                .Where(r => r.Type == type && r.Status == RequestStatus.Approved)
                .GroupBy(r => r.EmployeeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => (Start: r.StartDate.Date, End: r.EndDate.Date)).ToList());

        private static bool Covers(List<(DateTime Start, DateTime End)>? ranges, DateTime day) =>
            ranges is not null && ranges.Any(r => day >= r.Start && day <= r.End);

        // Most people arrive around 09:00; a long tail arrives later.
        private static TimeSpan RandomCheckIn(Random random)
        {
            int minutes = random.Next(100) switch
            {
                < 65 => random.Next(-20, 6),    // 08:40 - 09:05
                < 88 => random.Next(6, 31),     // 09:06 - 09:30
                < 97 => random.Next(31, 76),    // 09:31 - 10:15
                _ => random.Next(76, 151)       // properly late
            };

            return AttendanceRecord.WorkStart + TimeSpan.FromMinutes(minutes);
        }

        private static TimeSpan RandomCheckOut(Random random, TimeSpan checkIn)
        {
            int minutes = random.Next(100) switch
            {
                < 22 => random.Next(-90, 0),    // slipped out early
                < 78 => random.Next(0, 46),     // 17:00 - 17:45
                _ => random.Next(46, 121)       // stayed late
            };

            var checkOut = AttendanceRecord.WorkEnd + TimeSpan.FromMinutes(minutes);

            // Someone who arrived at 11:30 cannot have left at 15:30 and still
            // have a sane row — keep at least an hour between the two.
            var earliestSane = checkIn + TimeSpan.FromHours(1);
            return checkOut < earliestSane ? earliestSane : checkOut;
        }

        // Late wins over early-leave: if you did both, the table shows "Late".
        private static string Classify(TimeSpan checkIn, TimeSpan checkOut)
        {
            if ((checkIn - AttendanceRecord.WorkStart).TotalMinutes > AttendanceRecord.LateGraceMinutes)
                return AttendanceStatus.Late;

            if ((AttendanceRecord.WorkEnd - checkOut).TotalMinutes > AttendanceRecord.EarlyLeaveGraceMinutes)
                return AttendanceStatus.EarlyLeave;

            return AttendanceStatus.Present;
        }

        // ---- Queries --------------------------------------------------------

        public List<AttendanceRecord> Filter(AttendanceQuery query) =>
            Sort(ApplyFilters(query), query).ToList();

        public PagedResult<AttendanceRecord> Query(AttendanceQuery query) =>
            Filter(query).ToPagedResult(query.Page, query.PageSize);

        private IEnumerable<AttendanceRecord> ApplyFilters(AttendanceQuery query)
        {
            IEnumerable<AttendanceRecord> result = _records;

            // ?date= is the "one specific day" shortcut and beats ?from/?to.
            if (query.Date.HasValue)
            {
                var day = query.Date.Value.Date;
                result = result.Where(r => r.Date == day);
            }
            else
            {
                if (query.From.HasValue) result = result.Where(r => r.Date >= query.From.Value.Date);
                if (query.To.HasValue) result = result.Where(r => r.Date <= query.To.Value.Date);
            }

            if (query.EmployeeId.HasValue)
                result = result.Where(r => r.EmployeeId == query.EmployeeId.Value);

            // The page's dedicated "search by id" box — partial match, so typing
            // "201807" finds the whole July-2018 hiring cohort.
            if (!string.IsNullOrWhiteSpace(query.EmployeeCode))
                result = result.Where(r => r.EmployeeCode.Contains(query.EmployeeCode.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(query.Name))
                result = result.Where(r => r.EmployeeName.Contains(query.Name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(query.Department))
                result = result.Where(r => r.Department.Equals(query.Department, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(query.Status))
                result = result.Where(r => r.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));

            // The page's "any keyword" box — sweeps every text column at once.
            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var k = query.Keyword.Trim();
                result = result.Where(r =>
                    r.EmployeeName.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    r.EmployeeCode.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    r.Department.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    r.JobTitle.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    r.Status.Contains(k, StringComparison.OrdinalIgnoreCase));
            }

            return result;
        }

        private static IEnumerable<AttendanceRecord> Sort(IEnumerable<AttendanceRecord> source, AttendanceQuery query)
        {
            bool desc = query.Descending;

            return (query.Sort ?? string.Empty).ToLowerInvariant() switch
            {
                // The page's "latest / earliest attendance time" select. These two
                // carry their own direction, so ?sortDir is ignored for them.
                // `r.CheckIn == null` sorts false(0) before true(1), which parks
                // absent and on-leave rows at the bottom either way.
                "latestcheckin" => source.OrderBy(r => r.CheckIn == null).ThenByDescending(r => r.CheckIn),
                "earliestcheckin" => source.OrderBy(r => r.CheckIn == null).ThenBy(r => r.CheckIn),

                "date" => desc ? source.OrderByDescending(r => r.Date) : source.OrderBy(r => r.Date),
                "name" => desc ? source.OrderByDescending(r => r.EmployeeName) : source.OrderBy(r => r.EmployeeName),
                "lateminutes" => desc ? source.OrderByDescending(r => r.LateMinutes) : source.OrderBy(r => r.LateMinutes),
                "workedhours" => desc ? source.OrderByDescending(r => r.WorkedHours) : source.OrderBy(r => r.WorkedHours),
                "status" => desc ? source.OrderByDescending(r => r.Status) : source.OrderBy(r => r.Status),

                _ => source.OrderByDescending(r => r.Date).ThenBy(r => r.EmployeeName)
            };
        }

        public AttendanceSummary GetSummary(AttendanceQuery query) => Summarise(ApplyFilters(query));

        private static AttendanceSummary Summarise(IEnumerable<AttendanceRecord> source)
        {
            var records = source as IList<AttendanceRecord> ?? source.ToList();

            int present = records.Count(r => r.Status == AttendanceStatus.Present);
            int late = records.Count(r => r.Status == AttendanceStatus.Late);
            int earlyLeave = records.Count(r => r.Status == AttendanceStatus.EarlyLeave);
            int absent = records.Count(r => r.Status == AttendanceStatus.Absent);
            int onLeave = records.Count(r => r.Status == AttendanceStatus.OnLeave);
            int remote = records.Count(r => r.Status == AttendanceStatus.Remote);

            // Approved leave is not a missed day — it is excluded from BOTH sides
            // of the ratio, so a month with lots of holidays does not look bad.
            int scheduled = records.Count - onLeave;
            int attended = scheduled - absent;

            var checkIns = records.Where(r => r.CheckIn.HasValue).Select(r => r.CheckIn!.Value.Ticks).ToList();
            string? averageCheckIn = checkIns.Count == 0
                ? null
                : new TimeSpan((long)checkIns.Average()).ToString(@"hh\:mm");

            return new AttendanceSummary(
                Total: records.Count,
                Present: present,
                Late: late,
                EarlyLeave: earlyLeave,
                Absent: absent,
                OnLeave: onLeave,
                Remote: remote,
                AttendanceRate: Percent(attended, scheduled),
                PunctualityRate: Percent(attended - late, attended),
                AverageCheckIn: averageCheckIn,
                TotalLateMinutes: records.Sum(r => r.LateMinutes));
        }

        private static double Percent(int part, int whole) =>
            whole == 0 ? 0 : Math.Round(part * 100.0 / whole, 2);

        private double RateBetween(DateTime from, DateTime to)
        {
            var window = _records.Where(r => r.Date >= from.Date && r.Date <= to.Date).ToList();
            int onLeave = window.Count(r => r.Status == AttendanceStatus.OnLeave);
            int absent = window.Count(r => r.Status == AttendanceStatus.Absent);
            int scheduled = window.Count - onLeave;
            return Percent(scheduled - absent, scheduled);
        }

        public TrendStat GetMonthlyRateTrend(DateTime asOf)
        {
            var monthStart = new DateTime(asOf.Year, asOf.Month, 1);
            var previousMonthEnd = monthStart.AddDays(-1);
            var previousMonthStart = new DateTime(previousMonthEnd.Year, previousMonthEnd.Month, 1);

            // Current month is only partial, but a RATE is comparable to a full
            // month's rate — unlike a raw count, which would not be.
            return TrendStat.From(
                (decimal)RateBetween(monthStart, asOf),
                (decimal)RateBetween(previousMonthStart, previousMonthEnd));
        }

        public List<DailyAttendancePoint> GetTrend(int days)
        {
            days = Math.Clamp(days, 1, 180);
            var today = DateTime.Today;
            var from = today.AddDays(-(days - 1));

            return _records
                .Where(r => r.Date >= from && r.Date <= today)
                .GroupBy(r => r.Date)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    int onLeave = g.Count(r => r.Status == AttendanceStatus.OnLeave);
                    int absent = g.Count(r => r.Status == AttendanceStatus.Absent);
                    int scheduled = g.Count() - onLeave;

                    return new DailyAttendancePoint(
                        Date: g.Key,
                        Present: g.Count(r => r.Status == AttendanceStatus.Present),
                        Late: g.Count(r => r.Status == AttendanceStatus.Late),
                        Absent: absent,
                        OnLeave: onLeave,
                        Remote: g.Count(r => r.Status == AttendanceStatus.Remote),
                        AttendanceRate: Percent(scheduled - absent, scheduled));
                })
                .ToList();
        }

        public TeamStatusToday GetTodayStatus()
        {
            // On a Friday or Saturday there are no rows for "today" at all, so
            // fall back to the most recent day that does have data.
            var day = _records.Count == 0 ? DateTime.Today : _records.Max(r => r.Date);
            var todays = _records.Where(r => r.Date == day).ToList();

            return new TeamStatusToday(
                Date: day,
                InOffice: todays.Count(r => r.Status is AttendanceStatus.Present
                                                     or AttendanceStatus.Late
                                                     or AttendanceStatus.EarlyLeave),
                Remote: todays.Count(r => r.Status == AttendanceStatus.Remote),
                OnLeave: todays.Count(r => r.Status == AttendanceStatus.OnLeave),
                Absent: todays.Count(r => r.Status == AttendanceStatus.Absent),
                Total: todays.Count);
        }
    }
}
