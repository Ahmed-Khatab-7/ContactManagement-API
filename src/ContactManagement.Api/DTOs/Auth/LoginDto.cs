namespace ContactManagement.Api.DTOs.Auth;

public record LoginDto(
    string Email,
    string Password
);
