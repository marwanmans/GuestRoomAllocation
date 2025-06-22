// GuestRoomAllocation.Application/Common/Interfaces/IApplicationDbContext.cs
using GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GuestRoomAllocation.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Guest> Guests { get; }
    DbSet<Apartment> Apartments { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Allocation> Allocations { get; }
    DbSet<MaintenancePeriod> MaintenancePeriods { get; }
    DbSet<User> Users { get; }
    DbSet<UserApartmentAccess> UserApartmentAccess { get; }
    DbSet<UserGuestAccess> UserGuestAccess { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// GuestRoomAllocation.Application/Common/Interfaces/ICurrentUserService.cs
namespace GuestRoomAllocation.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}

// GuestRoomAllocation.Application/Common/Interfaces/IDateTime.cs
namespace GuestRoomAllocation.Application.Common.Interfaces;

public interface IDateTime
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateTime Today { get; }
}

// GuestRoomAllocation.Application/Common/Interfaces/INotificationService.cs
namespace GuestRoomAllocation.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAllocationConfirmationAsync(int allocationId, CancellationToken cancellationToken = default);
    Task SendMaintenanceNotificationAsync(int maintenancePeriodId, CancellationToken cancellationToken = default);
    Task SendAllocationReminderAsync(int allocationId, CancellationToken cancellationToken = default);
}

// GuestRoomAllocation.Application/Common/Models/Result.cs
namespace GuestRoomAllocation.Application.Common.Models;

public class Result
{
    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; set; }
    public string[] Errors { get; set; }

    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }
}

public class Result<T> : Result
{
    internal Result(bool succeeded, T data, IEnumerable<string> errors)
        : base(succeeded, errors)
    {
        Data = data;
    }

    public T Data { get; set; }

    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, Array.Empty<string>());
    }

    public new static Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(false, default!, errors);
    }
}

// GuestRoomAllocation.Application/Common/Models/PaginatedList.cs
using Microsoft.EntityFrameworkCore;

namespace GuestRoomAllocation.Application.Common.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}

// GuestRoomAllocation.Application/Common/Models/LookupDto.cs
namespace GuestRoomAllocation.Application.Common.Models;

public class LookupDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

// GuestRoomAllocation.Application/Common/Exceptions/ValidationException.cs
using FluentValidation.Results;

namespace GuestRoomAllocation.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}

// GuestRoomAllocation.Application/Common/Exceptions/NotFoundException.cs
namespace GuestRoomAllocation.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException()
        : base()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}