namespace ContactManagement.Api.DTOs.Auth;

public record AuthResponseDto(
    bool Succeeded,
    string? Token = null,
    DateTime? ExpiresAt = null,
    string? UserId = null,
    string? Email = null,
    IEnumerable<string>? Errors = null
);
