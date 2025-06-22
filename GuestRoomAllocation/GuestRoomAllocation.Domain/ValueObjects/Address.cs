// GuestRoomAllocation.Domain/ValueObjects/Address.cs
using GuestRoomAllocation.Domain.Exceptions;
using GuestRoomAllocation.Domain.ValueObjects.GuestRoomAllocation.Domain.ValueObjects.GuestRoomAllocation.Domain.ValueObjects.GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }

    private Address() { } // EF Core

    public Address(string street, string city, string state, string zipCode, string country)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = state ?? throw new ArgumentNullException(nameof(state));
        ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
        Country = country ?? throw new ArgumentNullException(nameof(country));
    }

    public override string ToString()
        => $"{Street}, {City}, {State} {ZipCode}, {Country}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }
}

// GuestRoomAllocation.Domain/ValueObjects/ContactInfo.cs
namespace GuestRoomAllocation.Domain.ValueObjects;

public class ContactInfo : ValueObject
{
    public string Email { get; private set; }
    public string Phone { get; private set; }

    private ContactInfo() { } // EF Core

    public ContactInfo(string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty.", nameof(phone));

        Email = email;
        Phone = phone;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Email;
        yield return Phone;
    }
}

// GuestRoomAllocation.Domain/ValueObjects/DateRange.cs
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

// GuestRoomAllocation.Domain/ValueObjects/ValueObject.cs
namespace GuestRoomAllocation.Domain.ValueObjects;

public abstract class ValueObject
{
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            return false;

        return ReferenceEquals(left, right) || left!.Equals(right);
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !(EqualOperator(left, right));
    }

    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject one, ValueObject two)
    {
        return EqualOperator(one, two);
    }

    public static bool operator !=(ValueObject one, ValueObject two)
    {
        return NotEqualOperator(one, two);
    }
}