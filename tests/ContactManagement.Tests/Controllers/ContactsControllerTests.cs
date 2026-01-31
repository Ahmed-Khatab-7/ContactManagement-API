using System.Security.Claims;
using ContactManagement.Api.Controllers;
using ContactManagement.Api.DTOs.Contacts;
using ContactManagement.Api.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ContactManagement.Tests.Controllers;

public class ContactsControllerTests
{
    private readonly Mock<IContactService> _contactServiceMock;
    private readonly ContactsController _sut;
    private readonly string _userId;

    public ContactsControllerTests()
    {
        _contactServiceMock = new Mock<IContactService>();
        _sut = new ContactsController(_contactServiceMock.Object);
        _userId = Guid.NewGuid().ToString();
        
        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _userId),
            new(ClaimTypes.Email, "test@example.com")
        };
        
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    #region GetContacts Tests (2 tests)

    [Fact]
    public async Task GetContacts_ReturnsOkWithPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResultDto<ContactDto>(
            Items: new List<ContactDto>
            {
                new(1, "John", "Doe", "john@test.com", null, null, null, null),
                new(2, "Jane", "Smith", "jane@test.com", null, null, null, null)
            },
            TotalCount: 2,
            Page: 1,
            PageSize: 10,
            TotalPages: 1
        );

        _contactServiceMock
            .Setup(x => x.GetContactsAsync(_userId, 1, 10, null, false, null))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.GetContacts();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedData = okResult.Value.Should().BeOfType<PagedResultDto<ContactDto>>().Subject;
        returnedData.Items.Should().HaveCount(2);
        returnedData.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetContacts_WithParameters_PassesCorrectValues()
    {
        // Arrange
        var pagedResult = new PagedResultDto<ContactDto>(
            Items: new List<ContactDto>(),
            TotalCount: 0,
            Page: 2,
            PageSize: 20,
            TotalPages: 0
        );

        _contactServiceMock
            .Setup(x => x.GetContactsAsync(_userId, 2, 20, "name", true, "search"))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.GetContacts(
            page: 2, 
            pageSize: 20, 
            sortBy: "name", 
            sortDescending: true, 
            search: "search"
        );

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _contactServiceMock.Verify(
            x => x.GetContactsAsync(_userId, 2, 20, "name", true, "search"), 
            Times.Once
        );
    }

    #endregion

    #region GetContact Tests (2 tests)

    [Fact]
    public async Task GetContact_WithValidId_ReturnsOk()
    {
        // Arrange
        var contact = new ContactDto(1, "John", "Doe", "john@test.com", null, null, null, null);

        _contactServiceMock
            .Setup(x => x.GetContactByIdAsync(_userId, 1))
            .ReturnsAsync(contact);

        // Act
        var result = await _sut.GetContact(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContact = okResult.Value.Should().BeOfType<ContactDto>().Subject;
        returnedContact.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetContact_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _contactServiceMock
            .Setup(x => x.GetContactByIdAsync(_userId, 999))
            .ReturnsAsync((ContactDto?)null);

        // Act
        var result = await _sut.GetContact(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region CreateContact Tests (1 test)

    [Fact]
    public async Task CreateContact_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateContactDto("John", "Doe", "john@test.com", null, null, null, null);
        var createdContact = new ContactDto(1, "John", "Doe", "john@test.com", null, null, null, null);

        _contactServiceMock
            .Setup(x => x.CreateContactAsync(_userId, createDto))
            .ReturnsAsync(createdContact);

        // Act
        var result = await _sut.CreateContact(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_sut.GetContact));
        createdResult.RouteValues!["id"].Should().Be(1);
        
        var returnedContact = createdResult.Value.Should().BeOfType<ContactDto>().Subject;
        returnedContact.Id.Should().Be(1);
    }

    #endregion

    #region UpdateContact Tests (2 tests)

    [Fact]
    public async Task UpdateContact_WithValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateContactDto("Updated", "Name", "updated@test.com", null, null, null, null);
        var updatedContact = new ContactDto(1, "Updated", "Name", "updated@test.com", null, null, null, null);

        _contactServiceMock
            .Setup(x => x.UpdateContactAsync(_userId, 1, updateDto))
            .ReturnsAsync(updatedContact);

        // Act
        var result = await _sut.UpdateContact(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContact = okResult.Value.Should().BeOfType<ContactDto>().Subject;
        returnedContact.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateContact_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateContactDto("Test", "Test", "test@test.com", null, null, null, null);

        _contactServiceMock
            .Setup(x => x.UpdateContactAsync(_userId, 999, updateDto))
            .ReturnsAsync((ContactDto?)null);

        // Act
        var result = await _sut.UpdateContact(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DeleteContact Tests (1 test)

    [Fact]
    public async Task DeleteContact_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _contactServiceMock
            .Setup(x => x.DeleteContactAsync(_userId, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteContact(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    #endregion
}
