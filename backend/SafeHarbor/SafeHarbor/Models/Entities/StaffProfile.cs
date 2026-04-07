namespace SafeHarbor.Models.Entities;

public class StaffProfile : AuditableEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<StaffRole> UserRoles { get; set; } = new List<StaffRole>();
}
