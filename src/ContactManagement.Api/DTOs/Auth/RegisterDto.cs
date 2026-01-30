namespace ContactManagement.Api.DTOs.Auth;

public record RegisterDto(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName
);
