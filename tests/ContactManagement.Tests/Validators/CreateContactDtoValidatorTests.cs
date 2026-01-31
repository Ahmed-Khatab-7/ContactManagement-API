using ContactManagement.Api.DTOs.Contacts;
using ContactManagement.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace ContactManagement.Tests.Validators;

public class CreateContactDtoValidatorTests
{
    private readonly CreateContactDtoValidator _validator;

    public CreateContactDtoValidatorTests()
    {
        _validator = new CreateContactDtoValidator();
    }

    [Fact]
    public async Task FirstName_WhenEmpty_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateContactDto("", "Doe", "test@test.com", null, null, null, null);

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First name is required");
    }

    [Fact]
    public async Task Email_WhenInvalid_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateContactDto("John", "Doe", "invalid-email", null, null, null, null);

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public async Task BirthDate_WhenInFuture_ShouldHaveError()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var dto = new CreateContactDto("John", "Doe", "test@test.com", null, futureDate, null, null);

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
              .WithErrorMessage("Birth date cannot be in the future");
    }

    [Fact]
    public async Task PhoneNumber_WhenInvalidFormat_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateContactDto("John", "Doe", "test@test.com", "abc123xyz", null, null, null);

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
              .WithErrorMessage("Invalid phone number format");
    }

    [Fact]
    public async Task ValidDto_ShouldPassAllValidations()
    {
        // Arrange
        var dto = new CreateContactDto(
            FirstName: "John",
            LastName: "Doe",
            Email: "john@example.com",
            PhoneNumber: "+1-555-123-4567",
            BirthDate: new DateOnly(1990, 5, 15),
            Address: "123 Main St",
            Notes: "Test notes"
        );

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
