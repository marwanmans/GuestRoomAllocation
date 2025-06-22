// GuestRoomAllocation.Domain/Exceptions/DomainException.cs
namespace GuestRoomAllocation.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

// GuestRoomAllocation.Domain/Exceptions/RoomNotAvailableException.cs
namespace GuestRoomAllocation.Domain.Exceptions;

public class RoomNotAvailableException : DomainException
{
    public RoomNotAvailableException(int roomId, DateTime startDate, DateTime endDate)
        : base($"Room {roomId} is not available from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}")
    {
    }
}

// GuestRoomAllocation.Domain/Exceptions/InvalidDateRangeException.cs
namespace GuestRoomAllocation.Domain.Exceptions;

public class InvalidDateRangeException : DomainException
{
    public InvalidDateRangeException(string message) : base(message)
    {
    }
}

// GuestRoomAllocation.Domain/Exceptions/GuestNotFoundException.cs
namespace GuestRoomAllocation.Domain.Exceptions;

public class GuestNotFoundException : DomainException
{
    public GuestNotFoundException(int guestId)
        : base($"Guest with ID {guestId} was not found")
    {
    }
}

// GuestRoomAllocation.Domain/Exceptions/RoomNotFoundException.cs
namespace GuestRoomAllocation.Domain.Exceptions;

public class RoomNotFoundException : DomainException
{
    public RoomNotFoundException(int roomId)
        : base($"Room with ID {roomId} was not found")
    {
    }
}

// GuestRoomAllocation.Domain/Exceptions/AllocationNotFoundException.cs
namespace GuestRoomAllocation.Domain.Exceptions;

public class AllocationNotFoundException : DomainException
{
    public AllocationNotFoundException(int allocationId)
        : base($"Allocation with ID {allocationId} was not found")
    {
    }
}