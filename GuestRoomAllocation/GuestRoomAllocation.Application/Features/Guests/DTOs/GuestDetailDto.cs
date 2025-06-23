namespace GuestRoomAllocation.Application.Features.Guests.DTOs;

public class GuestDetailDto : GuestDto
{
    public int TotalAllocations { get; set; }
    public int CurrentAllocations { get; set; }
    public DateTime? LastModified { get; set; }
}