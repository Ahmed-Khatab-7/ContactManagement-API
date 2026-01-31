using ContactManagement.Api.DTOs.Auth;
using ContactManagement.Api.Models;
using ContactManagement.Api.Services;
using ContactManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ContactManagement.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _configurationMock = new Mock<IConfiguration>();
        SetupJwtConfiguration();

        _sut = new AuthService(
            _userManagerMock.Object,
            _configurationMock.Object,
            TestHelpers.CreateMockLogger<AuthService>()
        );
    }

    private void SetupJwtConfiguration()
    {
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["SecretKey"]).Returns("ThisIsAVeryLongSecretKeyForTestingPurposesOnly12345!");
        jwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
        jwtSection.Setup(x => x["Audience"]).Returns("TestAudience");
        jwtSection.Setup(x => x["ExpirationInMinutes"]).Returns("60");
        
        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);
    }

    #region RegisterAsync Tests (3 tests)

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsSuccessWithToken()
    {
        // Arrange
        var dto = new RegisterDto(
            Email: "newuser@example.com",
            Password: "SecurePass123!",
            FirstName: "New",
            LastName: "User"
        );

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(dto.Email);
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFailure()
    {
        // Arrange
        var dto = new RegisterDto(
            Email: "existing@example.com",
            Password: "SecurePass123!",
            FirstName: "Existing",
            LastName: "User"
        );

        var existingUser = TestHelpers.CreateTestUser(email: dto.Email);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("A user with this email already exists");
    }

    [Fact]
    public async Task RegisterAsync_WhenCreateFails_ReturnsIdentityErrors()
    {
        // Arrange
        var dto = new RegisterDto(
            Email: "newuser@example.com",
            Password: "weak",
            FirstName: "New",
            LastName: "User"
        );

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var identityErrors = new[]
        {
            new IdentityError { Description = "Password too weak" },
            new IdentityError { Description = "Password requires uppercase" }
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Password too weak");
    }

    #endregion

    #region LoginAsync Tests (3 tests)

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var dto = new LoginDto(
            Email: "test@example.com",
            Password: "ValidPass123!"
        );

        var user = TestHelpers.CreateTestUser(email: dto.Email);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(dto.Email);
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var dto = new LoginDto(
            Email: "nonexistent@example.com",
            Password: "AnyPassword123!"
        );

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        var dto = new LoginDto(
            Email: "test@example.com",
            Password: "WrongPassword123!"
        );

        var user = TestHelpers.CreateTestUser(email: dto.Email);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Invalid email or password");
    }

    #endregion
}
