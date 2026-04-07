namespace SafeHarbor.Models.Entities;

public class StaffRole
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Add this line!

    public Guid StaffProfileId { get; set; }
    public Guid RoleId { get; set; }

    public StaffProfile? StaffProfile { get; set; }
    public Role? Role { get; set; }
}