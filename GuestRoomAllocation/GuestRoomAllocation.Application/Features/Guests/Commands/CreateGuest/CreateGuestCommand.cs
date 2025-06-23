using MediatR;
using GuestRoomAllocation.Application.Common.Models;

namespace GuestRoomAllocation.Application.Features.Guests.Commands.CreateGuest;

public class CreateGuestCommand : IRequest<Result<int>>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? JobPosition { get; set; }
    public string? Notes { get; set; }
}