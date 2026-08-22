namespace PeopleHub.Data
{
    // Same idea as Data/Departments.cs: one source of truth used BOTH to seed
    // the mock requests AND to serve /api/lookups/request-types, so the values
    // in the filter dropdown are guaranteed to be values that actually exist in
    // the data. No spaces in the values — they travel in query strings.
    public static class RequestCatalog
    {
        public const string Leave = "Leave";
        public const string RemoteWork = "RemoteWork";
        public const string Overtime = "Overtime";
        public const string Loan = "Loan";
        public const string Resignation = "Resignation";

        public static readonly string[] Types = { Leave, RemoteWork, Overtime, Loan, Resignation };

        public static readonly string[] LeaveTypes =
            { "Annual", "Sick", "Unpaid", "Maternity", "Bereavement" };

        public static readonly Dictionary<string, string[]> ReasonsByType = new()
        {
            [Leave] = new[]
            {
                "Family vacation", "Medical appointment", "Feeling unwell",
                "Personal matters", "Travelling abroad", "Wedding in the family",
                "Moving house", "Caring for a sick relative", "Exam period"
            },
            [RemoteWork] = new[]
            {
                "Working from home this week", "Internet installation at home",
                "Car in the garage", "Focus time on a deliverable", "Child care"
            },
            [Overtime] = new[]
            {
                "Month-end closing", "Production release", "Client deadline",
                "Covering for a colleague", "Quarterly audit preparation"
            },
            [Loan] = new[]
            {
                "Salary advance", "Medical expenses", "School fees",
                "Home renovation", "Car repair"
            },
            [Resignation] = new[]
            {
                "Accepted another offer", "Relocating to another city",
                "Returning to study", "Personal reasons"
            }
        };
    }
}
