namespace PeopleHub.Models
{
    // The single envelope every list endpoint returns (employees, attendance,
    // requests, salaries). One shape means the frontend can write one generic
    // table/pagination component instead of one per page.
    //
    // TotalCount = rows matching the filters BEFORE paging (what the UI shows
    // as "245 results"); Items = just the slice for the requested page.
    public record PagedResult<T>(
        List<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);
}
