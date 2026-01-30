namespace ContactManagement.Api.DTOs.Auth;

public record RegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName
);