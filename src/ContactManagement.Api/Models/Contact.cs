namespace ContactManagement.Api.Models;


public class Contact
{
    private Contact() { }

    public static Contact Create(
        string firstName,
        string lastName,
        string email,
        string userId,
        string? phoneNumber = null,
        DateOnly? birthDate = null,
        string? address = null,
        string? notes = null)
    {
        return new Contact
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLower(),
            PhoneNumber = phoneNumber?.Trim(),
            BirthDate = birthDate,
            Address = address?.Trim(),
            Notes = notes?.Trim(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public int Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? Address { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string UserId { get; private set; } = string.Empty;

    // Soft Delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Navigation
    public virtual ApplicationUser User { get; private set; } = null!;

    // Update method
    public void Update(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        DateOnly? birthDate,
        string? address,
        string? notes)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLower();
        PhoneNumber = phoneNumber?.Trim();
        BirthDate = birthDate;
        Address = address?.Trim();
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    // Soft delete method
    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    // Restore method
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}