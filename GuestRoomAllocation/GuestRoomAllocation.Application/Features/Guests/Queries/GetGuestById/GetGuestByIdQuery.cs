using MediatR;
using GuestRoomAllocation.Application.Features.Guests.DTOs;

namespace GuestRoomAllocation.Application.Features.Guests.Queries.GetGuestById;

public class GetGuestByIdQuery : IRequest<GuestDetailDto?>
{
    public int Id { get; set; }

    public GetGuestByIdQuery(int id)
    {
        Id = id;
    }
}