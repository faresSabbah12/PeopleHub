using PeopleHub.Data;
using PeopleHub.Helpers;
using PeopleHub.Models;

namespace PeopleHub.Services
{
    // Mock "database": a List<Employee> generated once in memory, seeded with
    // a fixed Random seed so you get the SAME 200 fake employees every time
    // the API restarts (handy while building the UI against it).
    public class EmployeeService : IEmployeeService
    {
        // The company opened in May 2011 — nobody can have been hired earlier.
        public static readonly DateTime CompanyFoundedOn = new(2011, 5, 1);
        public const int SeedCount = 200;

        private static readonly string[] MaleFirstNames =
        {
            "Ahmad", "Mohammad", "Omar", "Khaled", "Yousef", "Hassan", "Ali", "Mahmoud",
            "Saeed", "Bilal", "Tariq", "Nasser", "Fadi", "Rami", "Zaid", "Karim",
            "Mustafa", "Anas", "Hamza", "Adnan", "Fares", "Rashed", "Raed"
        };

        private static readonly string[] FemaleFirstNames =
        {
            "Sara", "Laila", "Nour", "Rana", "Dana", "Reem", "Hala", "Maha",
            "Lina", "Yasmin", "Huda", "Rasha", "Salma", "Farah", "Iman", "Dina",
            "Amal", "Mona", "Ruba", "Zainab"
        };

        private static readonly string[] FamilyNames =
        {
            "Khatib", "Masri", "Hijazi", "Nashwan", "Abu Alia", "Khalil", "Saleh", "Zoubi",
            "Qasem", "Odeh", "Sweidan", "Barakat", "Hamdan", "Shawabkeh", "Tal", "Haddad",
            "Nazzal", "Mansour", "Hourani", "Qudah", "Rawashdeh", "Shaqran", " Sabbah"
        };

        private readonly List<Employee> _employees;
        private int _nextId;

        // Tracks how many people were hired in each "yyyyMM" cohort so
        // EmployeeCode sequences (…0001, …0002) never collide.
        private readonly Dictionary<string, int> _codeSequenceByCohort = new();

        public EmployeeService()
        {
            _employees = SeedEmployees(SeedCount);
            _nextId = _employees.Count + 1;
        }

        private List<Employee> SeedEmployees(int count)
        {
            var random = new Random(42);
            var employees = new List<Employee>(count);
            var usedEmails = new HashSet<string>();
            var rangeStart = CompanyFoundedOn;
            var rangeEnd = DateTime.Today;

            for (int i = 1; i <= count; i++)
            {
                bool isMale = random.Next(2) == 0;

                string firstName = isMale
                    ? MaleFirstNames[random.Next(MaleFirstNames.Length)]
                    : FemaleFirstNames[random.Next(FemaleFirstNames.Length)];
                string middleName = MaleFirstNames[random.Next(MaleFirstNames.Length)]; // father's name
                string lastName = FamilyNames[random.Next(FamilyNames.Length)];

                var hireDate = RandomDate(random, rangeStart, rangeEnd);
                var birthDate = RandomBirthDate(random, hireDate);

                string department = Departments.All[random.Next(Departments.All.Count)];
                var titles = Departments.JobTitlesByDepartment[department];
                string jobTitle = titles[random.Next(titles.Length)];

                int approxAge = DateTime.Today.Year - birthDate.Year;

                employees.Add(new Employee
                {
                    Id = i,
                    EmployeeCode = GenerateEmployeeCode(hireDate),
                    AvatarUrl = $"https://randomuser.me/api/portraits/{(isMale ? "men" : "women")}/{random.Next(0, 100)}.jpg",
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    Gender = isMale ? "Male" : "Female",
                    BirthDate = birthDate,
                    HireDate = hireDate,
                    Department = department,
                    JobTitle = jobTitle,
                    Salary = RandomSalary(random, jobTitle),
                    PhoneNumber = RandomPhoneNumber(random),
                    Email = BuildUniqueEmail(firstName, lastName, usedEmails),
                    MaritalStatus = RandomMaritalStatus(random, mostlySingle: approxAge < 28)
                });
            }

            return employees;
        }

        private string GenerateEmployeeCode(DateTime hireDate)
        {
            string cohort = hireDate.ToString("yyyyMM");
            int sequence = _codeSequenceByCohort.GetValueOrDefault(cohort) + 1;
            _codeSequenceByCohort[cohort] = sequence;
            return $"{cohort}{sequence:D4}"; // e.g. 2018070003
        }

        private static DateTime RandomDate(Random random, DateTime from, DateTime to)
        {
            int range = (to - from).Days;
            return from.AddDays(random.Next(range));
        }

        // Birthdate consistent with being 22-55 years old at the time of hire.
        private static DateTime RandomBirthDate(Random random, DateTime hireDate)
        {
            int ageAtHire = random.Next(22, 56);
            int year = hireDate.Year - ageAtHire;
            int month = random.Next(1, 13);
            int day = random.Next(1, DateTime.DaysInMonth(year, month) + 1);
            return new DateTime(year, month, day);
        }

        private static decimal RandomSalary(Random random, string jobTitle)
        {
            if (jobTitle.Contains("Manager")) return random.Next(1400, 2600);
            if (jobTitle.Contains("Senior") || jobTitle.Contains("Lead")) return random.Next(950, 1500);
            return random.Next(450, 950);
        }

        private static string RandomPhoneNumber(Random random)
        {
            string prefix = new[] { "077", "078", "079" }[random.Next(3)];
            return prefix + random.Next(0, 10_000_000).ToString("D7");
        }

        private static string RandomMaritalStatus(Random random, bool mostlySingle)
        {
            int roll = random.Next(100);
            if (mostlySingle) return roll < 80 ? "Single" : roll < 95 ? "Married" : "Divorced";
            return roll < 30 ? "Single" : roll < 90 ? "Married" : "Divorced";
        }

        private static string BuildUniqueEmail(string firstName, string lastName, HashSet<string> used)
        {
            static string Clean(string s) => s.Replace(" ", "").Replace("-", "").ToLowerInvariant();
            string email = $"{Clean(firstName)}.{Clean(lastName)}@peoplehub.com";
            int suffix = 1;
            while (!used.Add(email))
            {
                email = $"{Clean(firstName)}.{Clean(lastName)}{suffix}@peoplehub.com";
                suffix++;
            }
            return email;
        }

        // ---- Reads ----------------------------------------------------------

        public List<Employee> GetAll() => _employees;

        public Employee? GetById(int id) => _employees.FirstOrDefault(e => e.Id == id);

        public List<Employee> Filter(EmployeeQuery query)
        {
            // Start with everything, then narrow. Each `if` is one filter field
            // from the UI; skipping the ones the caller left empty is what makes
            // the filters combine freely.
            IEnumerable<Employee> result = _employees;

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var k = query.Keyword.Trim();
                result = result.Where(e =>
                    e.FullName.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    e.EmployeeCode.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    e.Email.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    e.PhoneNumber.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    e.Department.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    e.JobTitle.Contains(k, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Department))
                result = result.Where(e => e.Department.Equals(query.Department, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(query.JobTitle))
                result = result.Where(e => e.JobTitle.Equals(query.JobTitle, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(query.Gender))
                result = result.Where(e => e.Gender.Equals(query.Gender, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(query.MaritalStatus))
                result = result.Where(e => e.MaritalStatus.Equals(query.MaritalStatus, StringComparison.OrdinalIgnoreCase));

            if (query.MinSalary.HasValue) result = result.Where(e => e.Salary >= query.MinSalary.Value);
            if (query.MaxSalary.HasValue) result = result.Where(e => e.Salary <= query.MaxSalary.Value);

            if (query.HiredFrom.HasValue) result = result.Where(e => e.HireDate >= query.HiredFrom.Value.Date);
            if (query.HiredTo.HasValue) result = result.Where(e => e.HireDate <= query.HiredTo.Value.Date);

            return Sort(result, query).ToList();
        }

        private static IEnumerable<Employee> Sort(IEnumerable<Employee> source, EmployeeQuery query)
        {
            bool desc = query.Descending;

            // A `switch` expression: pick one branch by value, return it.
            return (query.Sort ?? string.Empty).ToLowerInvariant() switch
            {
                "name" => desc ? source.OrderByDescending(e => e.FullName) : source.OrderBy(e => e.FullName),
                "salary" => desc ? source.OrderByDescending(e => e.Salary) : source.OrderBy(e => e.Salary),
                "hiredate" => desc ? source.OrderByDescending(e => e.HireDate) : source.OrderBy(e => e.HireDate),
                "department" => desc ? source.OrderByDescending(e => e.Department) : source.OrderBy(e => e.Department),
                "jobtitle" => desc ? source.OrderByDescending(e => e.JobTitle) : source.OrderBy(e => e.JobTitle),
                "age" => desc ? source.OrderByDescending(e => e.Age) : source.OrderBy(e => e.Age),
                "code" => desc ? source.OrderByDescending(e => e.EmployeeCode) : source.OrderBy(e => e.EmployeeCode),
                _ => desc ? source.OrderByDescending(e => e.Id) : source.OrderBy(e => e.Id)
            };
        }

        public PagedResult<Employee> Query(EmployeeQuery query) =>
            Filter(query).ToPagedResult(query.Page, query.PageSize);

        // ---- Writes ---------------------------------------------------------

        public Employee Create(Employee employee)
        {
            employee.Id = _nextId++;
            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
                employee.EmployeeCode = GenerateEmployeeCode(employee.HireDate == default ? DateTime.Today : employee.HireDate);
            _employees.Add(employee);
            return employee;
        }

        public bool Update(int id, Employee updated)
        {
            var existing = GetById(id);
            if (existing is null) return false;

            existing.AvatarUrl = updated.AvatarUrl;
            existing.FirstName = updated.FirstName;
            existing.MiddleName = updated.MiddleName;
            existing.LastName = updated.LastName;
            existing.Gender = updated.Gender;
            existing.BirthDate = updated.BirthDate;
            existing.HireDate = updated.HireDate;
            existing.Department = updated.Department;
            existing.JobTitle = updated.JobTitle;
            existing.Salary = updated.Salary;
            existing.PhoneNumber = updated.PhoneNumber;
            existing.Email = updated.Email;
            existing.MaritalStatus = updated.MaritalStatus;
            return true;
        }

        public bool Delete(int id)
        {
            var existing = GetById(id);
            if (existing is null) return false;
            return _employees.Remove(existing);
        }

        // ---- Aggregates -----------------------------------------------------

        public int GetEmployeeCount() => _employees.Count;

        public decimal GetTotalSalary() => _employees.Sum(e => e.Salary);

        // Nobody in this mock data ever leaves, so headcount is simply "how many
        // people had been hired by this date". With real termination dates you
        // would also exclude anyone who had already left.
        public int GetHeadcountAsOf(DateTime date) =>
            _employees.Count(e => e.HireDate.Date <= date.Date);

        public TrendStat GetHeadcountTrend()
        {
            var today = DateTime.Today;
            return TrendStat.From(GetHeadcountAsOf(today), GetHeadcountAsOf(today.AddYears(-1)));
        }

        // Payroll for a year = the monthly wage bill added up month by month,
        // counting only people already hired at the start of each month. That is
        // why the year-over-year number moves even though nobody gets a raise:
        // the company had more staff on the books each month.
        public decimal GetPayrollForYear(int year, int throughMonth = 12)
        {
            var today = DateTime.Today;
            decimal total = 0;

            for (int month = 1; month <= Math.Clamp(throughMonth, 1, 12); month++)
            {
                var monthStart = new DateTime(year, month, 1);
                if (monthStart > today) break;  // the future has no payroll yet

                total += _employees.Where(e => e.HireDate <= monthStart).Sum(e => e.Salary);
            }

            return total;
        }

        public TrendStat GetPayrollTrend()
        {
            var today = DateTime.Today;
            // Same number of months on both sides, otherwise a partial current
            // year would always look like a collapse next to a full past year.
            return TrendStat.From(
                GetPayrollForYear(today.Year, today.Month),
                GetPayrollForYear(today.Year - 1, today.Month));
        }

        public List<YearlyPayrollPoint> GetPayrollByYear()
        {
            var today = DateTime.Today;
            var firstYear = _employees.Min(e => e.HireDate.Year);

            return Enumerable.Range(firstYear, today.Year - firstYear + 1)
                .Select(year => new YearlyPayrollPoint(
                    year,
                    GetPayrollForYear(year),
                    GetHeadcountAsOf(year == today.Year ? today : new DateTime(year, 12, 31))))
                .ToList();
        }

        public List<DepartmentHeadcount> GetHeadcountByDepartment()
        {
            int total = _employees.Count;

            return _employees
                .GroupBy(e => e.Department)
                .Select(g => new DepartmentHeadcount(
                    g.Key,
                    g.Count(),
                    total == 0 ? 0 : Math.Round(g.Count() * 100.0 / total, 2)))
                .OrderByDescending(d => d.Headcount)
                .ToList();
        }

        public SalarySummary SummariseSalaries(IEnumerable<Employee> employees)
        {
            var salaries = employees.Select(e => e.Salary).OrderBy(s => s).ToList();
            if (salaries.Count == 0)
                return new SalarySummary(0, 0, 0, 0, 0, 0, 0);

            decimal median = salaries.Count % 2 == 1
                ? salaries[salaries.Count / 2]
                : (salaries[salaries.Count / 2 - 1] + salaries[salaries.Count / 2]) / 2m;

            decimal total = salaries.Sum();

            return new SalarySummary(
                EmployeeCount: salaries.Count,
                TotalMonthly: total,
                TotalAnnual: total * 12,
                Average: Math.Round(total / salaries.Count, 2),
                Median: median,
                Min: salaries[0],
                Max: salaries[^1]);   // [^1] means "last item"
        }

        public List<DepartmentSalary> SummariseByDepartment(IEnumerable<Employee> employees)
        {
            var list = employees.ToList();
            decimal grandTotal = list.Sum(e => e.Salary);

            return list
                .GroupBy(e => e.Department)
                .Select(g =>
                {
                    decimal total = g.Sum(e => e.Salary);
                    return new DepartmentSalary(
                        g.Key,
                        g.Count(),
                        total,
                        Math.Round(total / g.Count(), 2),
                        grandTotal == 0 ? 0 : Math.Round((double)(total / grandTotal) * 100, 2));
                })
                .OrderByDescending(d => d.TotalMonthly)
                .ToList();
        }
    }
}
