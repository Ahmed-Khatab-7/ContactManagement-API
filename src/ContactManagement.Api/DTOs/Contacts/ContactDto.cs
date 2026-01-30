namespace ContactManagement.Api.DTOs.Contacts;

/// <summary>
/// DTO returned to client. Notice: No UserId exposed - that's internal!
/// </summary>
public record ContactDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateOnly? BirthDate,
    string? Address,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
