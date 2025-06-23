using GuestRoomAllocation.Application.Common.Interfaces;

namespace GuestRoomAllocation.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Today => DateTime.Today;
}
