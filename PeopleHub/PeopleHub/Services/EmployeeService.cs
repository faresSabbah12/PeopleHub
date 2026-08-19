using PeopleHub.Data;
using PeopleHub.Models;

namespace PeopleHub.Services
{
    // Mock "database": a List<Employee> generated once in memory, seeded with
    // a fixed Random seed so you get the SAME 120 fake employees every time
    // the API restarts (handy while building the UI against it).
    public class EmployeeService : IEmployeeService
    {
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
            _employees = SeedEmployees(120);
            _nextId = _employees.Count + 1;
        }

        private List<Employee> SeedEmployees(int count)
        {
            var random = new Random(42);
            var employees = new List<Employee>(count);
            var usedEmails = new HashSet<string>();
            var rangeStart = new DateTime(2013, 1, 1);
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

        public List<Employee> GetAll() => _employees;

        public Employee? GetById(int id) => _employees.FirstOrDefault(e => e.Id == id);

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

        public int GetEmployeeCount() => _employees.Count;

        public decimal GetTotalSalary() => _employees.Sum(e => e.Salary);
    }
}
