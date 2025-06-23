namespace GuestRoomAllocation.Application.Features.Guests.DTOs;

public class GuestDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? JobPosition { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
}