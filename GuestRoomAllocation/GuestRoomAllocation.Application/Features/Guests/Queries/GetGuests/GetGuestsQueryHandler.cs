// REPLACE FILE: Features/Guests/Queries/GetGuests/GetGuestsQueryHandler.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using GuestRoomAllocation.Application.Common.Interfaces;
using GuestRoomAllocation.Application.Common.Models;
using GuestRoomAllocation.Application.Features.Guests.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GuestRoomAllocation.Application.Features.Guests.Queries.GetGuests;

public class GetGuestsQueryHandler : IRequestHandler<GetGuestsQuery, PaginatedList<GuestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetGuestsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<GuestDto>> Handle(GetGuestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Guests.AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(g =>
                g.FirstName.Contains(request.SearchTerm) ||
                g.LastName.Contains(request.SearchTerm) ||
                g.ContactInfo.Email.Contains(request.SearchTerm));
        }

        var orderedQuery = query
            .OrderBy(g => g.LastName)
            .ThenBy(g => g.FirstName)
            .ProjectTo<GuestDto>(_mapper.ConfigurationProvider);

        return await PaginatedList<GuestDto>.CreateAsync(orderedQuery, request.PageNumber, request.PageSize);
    }
}