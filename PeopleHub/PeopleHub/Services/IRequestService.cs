using PeopleHub.Models;

namespace PeopleHub.Services
{
    public interface IRequestService
    {
        List<EmployeeRequest> GetAll();
        EmployeeRequest? GetById(int id);

        List<EmployeeRequest> Filter(RequestQuery query);
        PagedResult<EmployeeRequest> Query(RequestQuery query);

        EmployeeRequest Create(EmployeeRequest request, Employee employee);
        bool UpdateStatus(int id, string status, string? reviewedBy);
        bool Delete(int id);

        // Counts for the toggle badges. Deliberately ignores query.Status so the
        // Pending/Approved/Rejected numbers stay visible while one is selected.
        RequestSummary GetSummary(RequestQuery query);

        int GetOnLeaveCount(DateTime date);
        int GetActiveLeaveRequestCount();
        List<ActivityItem> GetRecentActivity(int take);
    }
}
