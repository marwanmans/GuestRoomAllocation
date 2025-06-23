namespace GuestRoomAllocation.Domain.Exceptions;

public class InvalidDateRangeException : DomainException
{
    public InvalidDateRangeException(string message) : base(message) { }
}
