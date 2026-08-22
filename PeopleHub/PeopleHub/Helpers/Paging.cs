using PeopleHub.Models;

namespace PeopleHub.Helpers
{
    // Extension method: lets you write `filtered.ToPagedResult(page, size)` on
    // ANY IEnumerable, as if List<T> had that method built in. `this` on the
    // first parameter is what makes it an extension.
    public static class Paging
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 200;

        public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int page, int pageSize)
        {
            // Never trust query-string numbers: ?page=-4&pageSize=999999 would
            // otherwise crash Skip() or dump 100k attendance rows in one response.
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize < 1 ? DefaultPageSize : pageSize, 1, MaxPageSize);

            // Materialise once — otherwise Count() and Skip() would each walk
            // the whole LINQ chain (filters + sort) a second time.
            var all = source as IList<T> ?? source.ToList();

            var totalCount = all.Count;
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
            var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<T>(items, page, pageSize, totalCount, totalPages);
        }
    }
}
