using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ContactManagement.Api.DTOs.Auth;
using ContactManagement.Api.Models;
using ContactManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ContactManagement.Api.Services;

/// <summary>
/// Handles user registration and authentication.
/// Uses UserManager for all user operations - never hash passwords manually!
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto(
                Succeeded: false,
                Errors: new[] { "A user with this email already exists" }
            );
        }

        // Create new user - UserManager handles password hashing automatically!
        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email, // Using email as username
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return new AuthResponseDto(
                Succeeded: false,
                Errors: result.Errors.Select(e => e.Description)
            );
        }

        _logger.LogInformation("User {Email} registered successfully", dto.Email);

        // Generate token for immediate login after registration
        var (token, expiresAt) = GenerateJwtToken(user);

        return new AuthResponseDto(
            Succeeded: true,
            Token: token,
            ExpiresAt: expiresAt,
            UserId: user.Id,
            Email: user.Email
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user == null)
        {
            // Don't reveal that the user doesn't exist (security best practice)
            return new AuthResponseDto(
                Succeeded: false,
                Errors: new[] { "Invalid email or password" }
            );
        }

        // Check password - UserManager compares hashed values
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        
        if (!isPasswordValid)
        {
            _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
            return new AuthResponseDto(
                Succeeded: false,
                Errors: new[] { "Invalid email or password" }
            );
        }

        _logger.LogInformation("User {Email} logged in successfully", dto.Email);

        var (token, expiresAt) = GenerateJwtToken(user);

        return new AuthResponseDto(
            Succeeded: true,
            Token: token,
            ExpiresAt: expiresAt,
            UserId: user.Id,
            Email: user.Email
        );
    }

    /// <summary>
    /// Generates JWT token with essential claims.
    /// CRITICAL: NameIdentifier claim contains the UserId - used for data isolation!
    /// </summary>
    private (string Token, DateTime ExpiresAt) GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey not configured");
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"] ?? "60");
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        // Claims embedded in the token
        var claims = new List<Claim>
        {
            // This is the CRUCIAL claim - used to identify the user in ContactService
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
