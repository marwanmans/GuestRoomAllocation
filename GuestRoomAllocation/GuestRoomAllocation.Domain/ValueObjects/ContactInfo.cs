namespace GuestRoomAllocation.Domain.ValueObjects;

public class ContactInfo : ValueObject
{
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;

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