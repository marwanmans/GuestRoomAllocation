using GuestRoomAllocation.Domain.Exceptions;

namespace GuestRoomAllocation.Domain.ValueObjects;

public class DateRange : ValueObject
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    private DateRange() { } // EF Core

    public DateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
            throw new InvalidDateRangeException("Start date must be before end date.");

        StartDate = startDate.Date;
        EndDate = endDate.Date;
    }

    public int Duration => (EndDate - StartDate).Days;

    public bool Overlaps(DateRange other)
    {
        return StartDate < other.EndDate && EndDate > other.StartDate;
    }

    public bool Contains(DateTime date)
    {
        return date.Date >= StartDate && date.Date < EndDate;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }
}