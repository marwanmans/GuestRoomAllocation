using MediatR;
using GuestRoomAllocation.Application.Common.Models;
using GuestRoomAllocation.Application.Features.Guests.DTOs;

namespace GuestRoomAllocation.Application.Features.Guests.Queries.GetGuests;

public class GetGuestsQuery : IRequest<PaginatedList<GuestDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
