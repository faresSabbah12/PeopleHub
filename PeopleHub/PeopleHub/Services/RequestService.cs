using PeopleHub.Data;
using PeopleHub.Helpers;
using PeopleHub.Models;

namespace PeopleHub.Services
{
    // Second mock "table": HR requests, generated once at startup for the last
    // two years and tied to the real seeded employees.
    //
    // It asks the constructor for IEmployeeService — that is dependency
    // injection again: this class never builds an EmployeeService itself, it
    // just declares what it needs and Program.cs supplies it.
    public class RequestService : IRequestService
    {
        public const int HistoryYears = 2;

        private readonly IEmployeeService _employeeService;
        private readonly List<EmployeeRequest> _requests;
        private int _nextId;

        public RequestService(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            _requests = Seed();
            _nextId = _requests.Count + 1;
        }

        // ---- Seeding --------------------------------------------------------

        private List<EmployeeRequest> Seed()
        {
            var random = new Random(7);   // different seed from employees, still fixed
            var today = DateTime.Today;
            var windowStart = today.AddYears(-HistoryYears);
            var requests = new List<EmployeeRequest>();
            int id = 1;

            foreach (var employee in _employeeService.GetAll())
            {
                // Nobody files requests before they are hired.
                var from = employee.HireDate.Date > windowStart ? employee.HireDate.Date : windowStart;
                int span = (today - from).Days;
                if (span < 1) continue;

                int count = random.Next(4, 11);

                for (int i = 0; i < count; i++)
                {
                    string type = PickType(random);

                    var start = from.AddDays(random.Next(span + 1));
                    var end = start.AddDays(DurationDays(random, type) - 1);

                    var submittedAt = start.AddDays(-random.Next(1, 22));
                    if (submittedAt < employee.HireDate.Date) submittedAt = employee.HireDate.Date;

                    string status = PickStatus(random, submittedAt, today);

                    // An approved resignation would mean the person left, but
                    // this mock roster never shrinks — so resignations only ever
                    // sit as Pending or Rejected.
                    if (type == RequestCatalog.Resignation && status == RequestStatus.Approved)
                        status = RequestStatus.Pending;

                    var reasons = RequestCatalog.ReasonsByType[type];

                    requests.Add(new EmployeeRequest
                    {
                        Id = id++,
                        EmployeeId = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        EmployeeName = employee.FullName,
                        Department = employee.Department,
                        JobTitle = employee.JobTitle,
                        AvatarUrl = employee.AvatarUrl,
                        Type = type,
                        LeaveType = type == RequestCatalog.Leave
                            ? RequestCatalog.LeaveTypes[random.Next(RequestCatalog.LeaveTypes.Length)]
                            : null,
                        StartDate = start,
                        EndDate = end,
                        Status = status,
                        Reason = reasons[random.Next(reasons.Length)],
                        Amount = type == RequestCatalog.Loan ? random.Next(200, 3001) : null,
                        SubmittedAt = submittedAt,
                        ReviewedAt = status == RequestStatus.Pending
                            ? null
                            : Min(submittedAt.AddDays(random.Next(1, 6)), today),
                        ReviewedBy = status == RequestStatus.Pending ? null : "HR Manager"
                    });
                }
            }

            return requests;
        }

        private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

        // Weighted pick — leave is by far the most common request in real HR.
        private static string PickType(Random random) => random.Next(100) switch
        {
            < 55 => RequestCatalog.Leave,
            < 75 => RequestCatalog.RemoteWork,
            < 90 => RequestCatalog.Overtime,
            < 98 => RequestCatalog.Loan,
            _ => RequestCatalog.Resignation
        };

        private static int DurationDays(Random random, string type) => type switch
        {
            RequestCatalog.Leave => random.Next(1, 15),
            RequestCatalog.RemoteWork => random.Next(1, 6),
            _ => 1
        };

        private static string PickStatus(Random random, DateTime submittedAt, DateTime today)
        {
            // Something filed in the last 10 days is probably still on HR's desk.
            if ((today - submittedAt).TotalDays <= 10)
                return random.Next(100) < 65 ? RequestStatus.Pending
                     : random.Next(100) < 75 ? RequestStatus.Approved
                     : RequestStatus.Rejected;

            return random.Next(100) switch
            {
                < 72 => RequestStatus.Approved,
                < 94 => RequestStatus.Rejected,
                _ => RequestStatus.Pending
            };
        }

        // ---- Reads ----------------------------------------------------------

        public List<EmployeeRequest> GetAll() => _requests;

        public EmployeeRequest? GetById(int id) => _requests.FirstOrDefault(r => r.Id == id);

        public List<EmployeeRequest> Filter(RequestQuery query) =>
            Sort(ApplyFilters(query, includeStatus: true), query).ToList();

        public PagedResult<EmployeeRequest> Query(RequestQuery query) =>
            Filter(query).ToPagedResult(query.Page, query.PageSize);

        private IEnumerable<EmployeeRequest> ApplyFilters(RequestQuery query, bool includeStatus)
        {
            // "This month" and friends. Filtering is on SubmittedAt — the page
            // asks for "requests FROM this month", i.e. when they were filed.
            var (from, to) = DateRanges.Resolve(query.Period, query.Date);

            IEnumerable<EmployeeRequest> result = _requests
                .Where(r => r.SubmittedAt.Date >= from.Date && r.SubmittedAt.Date <= to.Date);

            if (includeStatus && !string.IsNullOrWhiteSpace(query.Status))
                result = result.Where(r => r.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(query.Type))
                result = result.Where(r => r.Type.Equals(query.Type, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(query.LeaveType))
                result = result.Where(r => query.LeaveType.Equals(r.LeaveType, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(query.Department))
                result = result.Where(r => r.Department.Equals(query.Department, StringComparison.OrdinalIgnoreCase));

            if (query.EmployeeId.HasValue)
                result = result.Where(r => r.EmployeeId == query.EmployeeId.Value);

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var k = query.Keyword.Trim();
                result = result.Where(r =>
                    r.EmployeeName.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    r.EmployeeCode.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    r.Department.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    r.Type.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    r.Reason.Contains(k, StringComparison.OrdinalIgnoreCase));
            }

            return result;
        }

        private static IEnumerable<EmployeeRequest> Sort(IEnumerable<EmployeeRequest> source, RequestQuery query)
        {
            bool desc = query.Descending;

            return (query.Sort ?? string.Empty).ToLowerInvariant() switch
            {
                "startdate" => desc ? source.OrderByDescending(r => r.StartDate) : source.OrderBy(r => r.StartDate),
                "submittedat" => desc ? source.OrderByDescending(r => r.SubmittedAt) : source.OrderBy(r => r.SubmittedAt),
                "name" => desc ? source.OrderByDescending(r => r.EmployeeName) : source.OrderBy(r => r.EmployeeName),
                "days" => desc ? source.OrderByDescending(r => r.Days) : source.OrderBy(r => r.Days),
                "type" => desc ? source.OrderByDescending(r => r.Type) : source.OrderBy(r => r.Type),
                "status" => desc ? source.OrderByDescending(r => r.Status) : source.OrderBy(r => r.Status),
                // Default: newest first — that is what an inbox should show.
                _ => source.OrderByDescending(r => r.SubmittedAt).ThenByDescending(r => r.Id)
            };
        }

        public RequestSummary GetSummary(RequestQuery query)
        {
            var (from, to) = DateRanges.Resolve(query.Period, query.Date);
            var scoped = ApplyFilters(query, includeStatus: false).ToList();

            return new RequestSummary(
                Total: scoped.Count,
                Pending: scoped.Count(r => r.Status == RequestStatus.Pending),
                Approved: scoped.Count(r => r.Status == RequestStatus.Approved),
                Rejected: scoped.Count(r => r.Status == RequestStatus.Rejected),
                ByType: scoped.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.Count()),
                OnLeaveToday: GetOnLeaveCount(DateTime.Today),
                ActiveLeaveRequests: GetActiveLeaveRequestCount(),
                From: from,
                To: to);
        }

        // People whose approved leave covers this date. Distinct, because one
        // person could in principle have two overlapping approved requests.
        public int GetOnLeaveCount(DateTime date) => _requests
            .Where(r => r.Type == RequestCatalog.Leave
                     && r.Status == RequestStatus.Approved
                     && r.StartDate.Date <= date.Date
                     && r.EndDate.Date >= date.Date)
            .Select(r => r.EmployeeId)
            .Distinct()
            .Count();

        // "Active" = leave requests still waiting for a decision, whenever filed.
        public int GetActiveLeaveRequestCount() =>
            _requests.Count(r => r.Type == RequestCatalog.Leave && r.Status == RequestStatus.Pending);

        public List<ActivityItem> GetRecentActivity(int take) => _requests
            // A request's latest event is its review if reviewed, else its filing.
            .Select(r => new ActivityItem(
                r.Id, r.EmployeeId, r.EmployeeName, r.AvatarUrl, r.Type, r.Status,
                r.ReviewedAt ?? r.SubmittedAt,
                r.ReviewedAt.HasValue
                    ? $"{r.Type} request {r.Status.ToLowerInvariant()}"
                    : $"Submitted a {r.Type.ToLowerInvariant()} request"))
            .OrderByDescending(a => a.At)
            .ThenByDescending(a => a.RequestId)
            .Take(Math.Clamp(take, 1, 50))
            .ToList();

        // ---- Writes ---------------------------------------------------------

        public EmployeeRequest Create(EmployeeRequest request, Employee employee)
        {
            request.Id = _nextId++;

            // Trust the employee record for the display fields, not whatever the
            // caller posted — otherwise a client could file a request under a
            // real id but someone else's name.
            request.EmployeeId = employee.Id;
            request.EmployeeCode = employee.EmployeeCode;
            request.EmployeeName = employee.FullName;
            request.Department = employee.Department;
            request.JobTitle = employee.JobTitle;
            request.AvatarUrl = employee.AvatarUrl;

            // New requests always start unreviewed, whatever the body said.
            request.Status = RequestStatus.Pending;
            request.ReviewedAt = null;
            request.ReviewedBy = null;
            if (request.SubmittedAt == default) request.SubmittedAt = DateTime.Today;

            _requests.Add(request);
            return request;
        }

        public bool UpdateStatus(int id, string status, string? reviewedBy)
        {
            var existing = GetById(id);
            if (existing is null) return false;

            var normalised = RequestStatus.Normalise(status);
            if (normalised is null) return false;

            existing.Status = normalised;
            existing.ReviewedAt = normalised == RequestStatus.Pending ? null : DateTime.Today;
            existing.ReviewedBy = normalised == RequestStatus.Pending
                ? null
                : (string.IsNullOrWhiteSpace(reviewedBy) ? "HR Manager" : reviewedBy);

            return true;
        }

        public bool Delete(int id)
        {
            var existing = GetById(id);
            if (existing is null) return false;
            return _requests.Remove(existing);
        }
    }
}
