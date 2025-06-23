namespace GuestRoomAllocation.Domain.Exceptions;

public class RoomNotAvailableException : DomainException
{
    public RoomNotAvailableException(int roomId, DateTime startDate, DateTime endDate)
        : base($"Room {roomId} is not available from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}") { }
}
