namespace PeopleHub.Models
{
    // DTO = Data Transfer Object: the shape the CLIENT is allowed to send, which
    // is deliberately smaller than the full EmployeeRequest. The client cannot
    // post an Id, a Status, or someone else's name — the server fills those in.
    public record CreateRequestDto(
        int EmployeeId,
        string Type,
        DateTime StartDate,
        DateTime EndDate,
        string? LeaveType,
        string? Reason,
        decimal? Amount);

    public record UpdateStatusDto(string Status, string? ReviewedBy);
}
