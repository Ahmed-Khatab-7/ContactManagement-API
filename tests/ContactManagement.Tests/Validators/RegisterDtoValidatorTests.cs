using ContactManagement.Api.DTOs.Auth;
using ContactManagement.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace ContactManagement.Tests.Validators;

public class RegisterDtoValidatorTests
{
    private readonly RegisterDtoValidator _validator;

    public RegisterDtoValidatorTests()
    {
        _validator = new RegisterDtoValidator();
    }

    #region Email Validation (4 tests)

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Email_WhenEmpty_ShouldHaveError(string? email)
    {
        // Arrange
        var dto = new RegisterDto(email!, "ValidPass123!", "John", "Doe");

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid.com")]
    public async Task Email_WhenInvalidFormat_ShouldHaveError(string email)
    {
        // Arrange
        var dto = new RegisterDto(email, "ValidPass123!", "John", "Doe");

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public async Task Email_WhenTooLong_ShouldHaveError()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@test.com";
        var dto = new RegisterDto(longEmail, "ValidPass123!", "John", "Doe");

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Email_WhenValid_ShouldNotHaveError()
    {
        // Arrange
        var dto = new RegisterDto("valid@example.com", "ValidPass123!", "John", "Doe");

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Password Validation (5 tests)

    [Fact]
    public async Task Password_WhenEmpty_ShouldHaveError()
    {
        // Arrange
        var dto = new RegisterDto("test@test.com", "", "John", "Doe");

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password is required");
    }

    [Fact]
    public async Task Password_WhenTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new RegisterDto("test@test.com", "Ab1!", "John", "Doe");

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must be at least 8 characters");
    }

    [Fact]
    public async Task Password_WhenMissingUppercase_ShouldHaveError()
    {
        // Arrange
        var dto = new RegisterDto("test@test.com", "password123!", "John", "Doe");

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must contain at least one uppercase letter");
    }

    [Fact]
    public async Task Password_WhenMissingSpecialChar_ShouldHaveError()
    {
        // Arrange
        var dto = new RegisterDto("test@test.com", "Password123", "John", "Doe");

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must contain at least one special character");
    }

    [Fact]
    public async Task Password_WhenValid_ShouldNotHaveError()
    {
        // Arrange
        var dto = new RegisterDto("test@test.com", "ValidPass123!", "John", "Doe");

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region Name Validation (2 tests)

    [Theory]
    [InlineData("", "Doe")]
    [InlineData(null, "Doe")]
    public async Task FirstName_WhenEmpty_ShouldHaveError(string? firstName, string lastName)
    {
        // Arrange
        var dto = new RegisterDto("test@test.com", "ValidPass123!", firstName!, lastName);

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First name is required");
    }

    [Theory]
    [InlineData("John", "")]
    [InlineData("John", null)]
    public async Task LastName_WhenEmpty_ShouldHaveError(string firstName, string? lastName)
    {
        // Arrange
        var dto = new RegisterDto("test@test.com", "ValidPass123!", firstName, lastName!);

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Last name is required");
    }

    #endregion
}
