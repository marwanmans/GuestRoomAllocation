using MediatR;
using GuestRoomAllocation.Application.Common.Interfaces;
using GuestRoomAllocation.Application.Common.Models;
using GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GuestRoomAllocation.Application.Features.Guests.Commands.CreateGuest;

public class CreateGuestCommandHandler : IRequestHandler<CreateGuestCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public CreateGuestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateGuestCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingGuest = await _context.Guests
            .FirstOrDefaultAsync(g => g.ContactInfo.Email == request.Email, cancellationToken);

        if (existingGuest != null)
        {
            return Result<int>.Failure(new[] { "A guest with this email already exists." });
        }

        var contactInfo = new ContactInfo(request.Email, request.Phone);

        var guest = new Guest(
            request.FirstName,
            request.LastName,
            contactInfo,
            request.JobPosition,
            request.Notes);

        _context.Guests.Add(guest);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(guest.Id);
    }
}