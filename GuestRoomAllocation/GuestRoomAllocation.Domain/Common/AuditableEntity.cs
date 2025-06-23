namespace GuestRoomAllocation.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}