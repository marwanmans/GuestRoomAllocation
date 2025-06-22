// GuestRoomAllocation.Application/Features/Guests/Commands/CreateGuest/CreateGuestCommand.cs
using GuestRoomAllocation.Application.Common.Interfaces;
using GuestRoomAllocation.Application.Common.Interfaces.GuestRoomAllocation.Application.Common.Interfaces.GuestRoomAllocation.Application.Common.Interfaces.GuestRoomAllocation.Application.Common.Interfaces.GuestRoomAllocation.Application.Common.Models;
using GuestRoomAllocation.Application.Common.Interfaces.GuestRoomAllocation.Application.Common.Interfaces.GuestRoomAllocation.Application.Common.Interfaces.GuestRoomAllocation.Application.Common.Interfaces.GuestRoomAllocation.Application.Common.Models.GuestRoomAllocation.Application.Common.Models.GuestRoomAllocation.Application.Common.Models.GuestRoomAllocation.Application.Common.Exceptions.GuestRoomAllocation.Application.Common.Exceptions;
using GuestRoomAllocation.Application.Common.Models;
using GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.ValueObjects.GuestRoomAllocation.Domain.ValueObjects;
using MediatR;

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

// GuestRoomAllocation.Application/Features/Guests/Commands/UpdateGuest/UpdateGuestCommand.cs
using MediatR;
using GuestRoomAllocation.Application.Common.Models;

namespace GuestRoomAllocation.Application.Features.Guests.Commands.UpdateGuest;

public class UpdateGuestCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? JobPosition { get; set; }
    public string? Notes { get; set; }
}

// GuestRoomAllocation.Application/Features/Guests/Commands/DeleteGuest/DeleteGuestCommand.cs
using MediatR;
using GuestRoomAllocation.Application.Common.Models;

namespace GuestRoomAllocation.Application.Features.Guests.Commands.DeleteGuest;

public class DeleteGuestCommand : IRequest<Result>
{
    public int Id { get; set; }

    public DeleteGuestCommand(int id)
    {
        Id = id;
    }
}

// GuestRoomAllocation.Application/Features/Guests/Commands/CreateGuest/CreateGuestCommandHandler.cs
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

// GuestRoomAllocation.Application/Features/Guests/Commands/UpdateGuest/UpdateGuestCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Application.Common.Exceptions;
using GuestRoomAllocation.Application.Common.Interfaces;
using GuestRoomAllocation.Application.Common.Models;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Application.Features.Guests.Commands.UpdateGuest;

public class UpdateGuestCommandHandler : IRequestHandler<UpdateGuestCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateGuestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateGuestCommand request, CancellationToken cancellationToken)
    {
        var guest = await _context.Guests
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (guest == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Guest), request.Id);
        }

        // Check if email is being changed and if it already exists
        if (guest.ContactInfo.Email != request.Email)
        {
            var existingGuest = await _context.Guests
                .FirstOrDefaultAsync(g => g.ContactInfo.Email == request.Email && g.Id != request.Id, cancellationToken);

            if (existingGuest != null)
            {
                return Result.Failure(new[] { "A guest with this email already exists." });
            }
        }

        var newContactInfo = new ContactInfo(request.Email, request.Phone);

        guest.UpdatePersonalInfo(request.FirstName, request.LastName, request.JobPosition, request.Notes);
        guest.UpdateContactInfo(newContactInfo);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

// GuestRoomAllocation.Application/Features/Guests/Commands/DeleteGuest/DeleteGuestCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Application.Common.Exceptions;
using GuestRoomAllocation.Application.Common.Interfaces;
using GuestRoomAllocation.Application.Common.Models;

namespace GuestRoomAllocation.Application.Features.Guests.Commands.DeleteGuest;

public class DeleteGuestCommandHandler : IRequestHandler<DeleteGuestCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteGuestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteGuestCommand request, CancellationToken cancellationToken)
    {
        var guest = await _context.Guests
            .Include(g => g.Allocations)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (guest == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Guest), request.Id);
        }

        // Check for active allocations
        var activeAllocations = guest.Allocations
            .Where(a => a.Status == Domain.Enums.AllocationStatus.Current ||
                       a.Status == Domain.Enums.AllocationStatus.Upcoming)
            .ToList();

        if (activeAllocations.Any())
        {
            return Result.Failure(new[] { "Cannot delete guest with active or upcoming allocations." });
        }

        _context.Guests.Remove(guest);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

// GuestRoomAllocation.Application/Features/Guests/Commands/CreateGuest/CreateGuestCommandValidator.cs
using FluentValidation;

namespace GuestRoomAllocation.Application.Features.Guests.Commands.CreateGuest;

public class CreateGuestCommandValidator : AbstractValidator<CreateGuestCommand>
{
    public CreateGuestCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("First name is required and must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Last name is required and must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150)
            .WithMessage("A valid email address is required and must not exceed 150 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("Phone number is required and must not exceed 20 characters.");

        RuleFor(x => x.JobPosition)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.JobPosition))
            .WithMessage("Job position must not exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 500 characters.");
    }
}

// GuestRoomAllocation.Application/Features/Guests/Commands/UpdateGuest/UpdateGuestCommandValidator.cs
using FluentValidation;

namespace GuestRoomAllocation.Application.Features.Guests.Commands.UpdateGuest;

public class UpdateGuestCommandValidator : AbstractValidator<UpdateGuestCommand>
{
    public UpdateGuestCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Guest ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("First name is required and must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Last name is required and must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150)
            .WithMessage("A valid email address is required and must not exceed 150 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("Phone number is required and must not exceed 20 characters.");

        RuleFor(x => x.JobPosition)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.JobPosition))
            .WithMessage("Job position must not exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 500 characters.");
    }
}