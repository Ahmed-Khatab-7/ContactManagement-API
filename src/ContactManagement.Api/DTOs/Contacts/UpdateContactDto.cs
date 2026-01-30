namespace ContactManagement.Api.DTOs.Contacts;

public record UpdateContactDto(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateOnly? BirthDate,
    string? Address,
    string? Notes
);
