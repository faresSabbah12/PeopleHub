namespace PeopleHub.Helpers
{
    // Turns a ?period=day|month|year|all (+ optional ?date=) pair into a
    // concrete [from, to] window. Requests and Dashboard both need this, so it
    // lives in one place — otherwise "what does 'this month' mean" would be
    // answered slightly differently in each controller.
    public static class DateRanges
    {
        public static (DateTime From, DateTime To) Resolve(string? period, DateTime? date)
        {
            var anchor = (date ?? DateTime.Today).Date;

            return (period ?? "month").Trim().ToLowerInvariant() switch
            {
                "day" => (anchor, anchor),
                "year" => (new DateTime(anchor.Year, 1, 1), new DateTime(anchor.Year, 12, 31)),
                "all" => (DateTime.MinValue, DateTime.MaxValue),
                _ => (new DateTime(anchor.Year, anchor.Month, 1),
                      new DateTime(anchor.Year, anchor.Month, DateTime.DaysInMonth(anchor.Year, anchor.Month)))
            };
        }

        // The company works Sunday-Thursday (Jordan). Everything that generates
        // or counts "working days" reads this one method, so switching to a
        // Mon-Fri week is a one-line change.
        public static bool IsWorkday(DateTime date) =>
            date.DayOfWeek != DayOfWeek.Friday && date.DayOfWeek != DayOfWeek.Saturday;
    }
}
