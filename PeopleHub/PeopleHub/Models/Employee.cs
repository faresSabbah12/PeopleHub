namespace PeopleHub.Models
{
    public class Employee
    {
        public int Id { get; set; }

        // Realistic-looking internal ID: hire year (4) + hire month (2) + a
        // sequence number within that hire cohort (4) — e.g. "2018070003" =
        // the 3rd person hired in July 2018.
        public string EmployeeCode { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty; // father's name
        public string LastName { get; set; } = string.Empty;   // family name
        public string FullName => $"{FirstName} {MiddleName} {LastName}";

        public string Gender { get; set; } = string.Empty; // "Male" | "Female"

        public DateTime BirthDate { get; set; }
        // Computed, not stored — so it's always correct instead of going stale.
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        public DateTime HireDate { get; set; }
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public decimal Salary { get; set; } // monthly, in JD

        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty; // Single | Married | Divorced
    }
}
