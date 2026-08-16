namespace PeopleHub.Data
{
    // Single source of truth for departments (and their job titles). Used to
    // seed mock employees AND served via /api/lookups/departments, so every
    // page in the system (Employees form, filters, reports...) reads the same list
    // instead of each screen hardcoding its own copy.
    public static class Departments
    {
        public static readonly Dictionary<string, string[]> JobTitlesByDepartment = new()
        {
            ["Engineering"] = new[] { "Software Engineer", "Senior Software Engineer", "Frontend Developer", "Backend Developer", "DevOps Engineer", "QA Engineer", "Engineering Manager" },
            ["Human Resources"] = new[] { "HR Specialist", "HR Coordinator", "Recruiter", "HR Manager" },
            ["Finance"] = new[] { "Accountant", "Financial Analyst", "Payroll Specialist", "Finance Manager" },
            ["Sales"] = new[] { "Sales Representative", "Account Executive", "Sales Manager" },
            ["Marketing"] = new[] { "Marketing Specialist", "Content Creator", "SEO Specialist", "Marketing Manager" },
            ["Customer Support"] = new[] { "Support Agent", "Customer Success Specialist", "Support Team Lead" },
            ["IT"] = new[] { "IT Support Specialist", "System Administrator", "Network Engineer", "IT Manager" },
            ["Operations"] = new[] { "Operations Coordinator", "Logistics Specialist", "Operations Manager" },
        };

        public static readonly List<string> All = JobTitlesByDepartment.Keys.ToList();
    }
}
