namespace GuestRoomAllocation.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;

    private Address() { } // EF Core

    public Address(string street, string city, string country)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        City = city ?? throw new ArgumentNullException(nameof(city));
        Country = country ?? throw new ArgumentNullException(nameof(country));
    }

    public override string ToString()
        => $"{Street}, {City}, {Country}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Country;
    }
}