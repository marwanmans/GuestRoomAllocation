using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GuestRoomAllocation.Application.Features.Guests.Queries.GetGuests;
using GuestRoomAllocation.Application.Features.Guests.DTOs;
using GuestRoomAllocation.Application.Common.Models;

namespace GuestRoomAllocation.Web.Pages.Guests;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public PaginatedList<GuestDto> Guests { get; set; } = null!;

    public async Task OnGetAsync(int pageNumber = 1, string? searchTerm = null)
    {
        var query = new GetGuestsQuery
        {
            PageNumber = pageNumber,
            PageSize = 10,
            SearchTerm = searchTerm
        };

        Guests = await _mediator.Send(query);
    }
}