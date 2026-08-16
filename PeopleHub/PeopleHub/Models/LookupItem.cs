namespace PeopleHub.Models
{
    // Generic {id, name} shape for any dropdown/lookup list the API serves
    // (departments today; reuse the same shape for others later).
    public record LookupItem(int Id, string Name);
}
