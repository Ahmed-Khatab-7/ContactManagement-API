using ContactManagement.Api.Data;
using ContactManagement.Api.DTOs.Contacts;
using ContactManagement.Api.Models;
using ContactManagement.Api.Services;
using ContactManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactManagement.Tests.Services;

public class ContactServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ContactService _sut;
    private readonly string _userId;

    public ContactServiceTests()
    {
        _context = TestHelpers.CreateInMemoryDbContext();
        _sut = new ContactService(_context, TestHelpers.CreateMockLogger<ContactService>());
        _userId = Guid.NewGuid().ToString();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetContactsAsync Tests (6 tests)

    [Fact]
    public async Task GetContactsAsync_WithNoContacts_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.GetContactsAsync(_userId);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetContactsAsync_WithContacts_ReturnsUserContactsOnly()
    {
        // Arrange
        var otherUserId = Guid.NewGuid().ToString();
        await TestHelpers.SeedContactsAsync(_context, _userId, 3);
        await TestHelpers.SeedContactsAsync(_context, otherUserId, 5);

        // Act
        var result = await _sut.GetContactsAsync(_userId);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetContactsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await TestHelpers.SeedContactsAsync(_context, _userId, 15);

        // Act
        var result = await _sut.GetContactsAsync(_userId, page: 2, pageSize: 5);

        // Assert
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetContactsAsync_WithSortByName_ReturnsSortedContacts()
    {
        // Arrange
        var contact1 = Contact.Create("Zara", "Adams", $"zara_{Guid.NewGuid():N}@test.com", _userId);
        var contact2 = Contact.Create("Alice", "Brown", $"alice_{Guid.NewGuid():N}@test.com", _userId);
        var contact3 = Contact.Create("Mike", "Wilson", $"mike_{Guid.NewGuid():N}@test.com", _userId);
        
        _context.Contacts.AddRange(contact1, contact2, contact3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetContactsAsync(_userId, sortBy: "name");

        // Assert
        var items = result.Items.ToList();
        items[0].LastName.Should().Be("Adams");
        items[1].LastName.Should().Be("Brown");
        items[2].LastName.Should().Be("Wilson");
    }

    [Fact]
    public async Task GetContactsAsync_WithSearch_ReturnsFilteredContacts()
    {
        // Arrange
        var contact1 = Contact.Create("John", "Doe", $"john_{Guid.NewGuid():N}@test.com", _userId);
        var contact2 = Contact.Create("Jane", "Smith", $"jane_{Guid.NewGuid():N}@test.com", _userId);
        var contact3 = Contact.Create("Johnny", "Walker", $"johnny_{Guid.NewGuid():N}@test.com", _userId);
        
        _context.Contacts.AddRange(contact1, contact2, contact3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetContactsAsync(_userId, searchTerm: "john");

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.All(c => c.FirstName.Contains("John", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task GetContactsAsync_ExcludesSoftDeletedContacts()
    {
        // Arrange
        await TestHelpers.SeedContactsAsync(_context, _userId, 3);
        var contactToDelete = await _context.Contacts.FirstAsync();
        contactToDelete.Delete();
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetContactsAsync(_userId);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    #endregion

    #region GetContactByIdAsync Tests (3 tests)

    [Fact]
    public async Task GetContactByIdAsync_WithValidId_ReturnsContact()
    {
        // Arrange
        var contact = await TestHelpers.SeedSingleContactAsync(_context, _userId);

        // Act
        var result = await _sut.GetContactByIdAsync(_userId, contact.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(contact.Id);
    }

    [Fact]
    public async Task GetContactByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetContactByIdAsync(_userId, 999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContactByIdAsync_WithOtherUsersContact_ReturnsNull()
    {
        // Arrange
        var otherUserId = Guid.NewGuid().ToString();
        var contact = await TestHelpers.SeedSingleContactAsync(_context, otherUserId);

        // Act
        var result = await _sut.GetContactByIdAsync(_userId, contact.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateContactAsync Tests (1 test)

    [Fact]
    public async Task CreateContactAsync_WithValidData_CreatesContact()
    {
        // Arrange
        var dto = new CreateContactDto(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            PhoneNumber: "+1-555-123-4567",
            BirthDate: new DateOnly(1990, 5, 15),
            Address: "123 Main St",
            Notes: "Test note"
        );

        // Act
        var result = await _sut.CreateContactAsync(_userId, dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");

        // Verify in database
        var dbContact = await _context.Contacts.FindAsync(result.Id);
        dbContact.Should().NotBeNull();
    }

    #endregion

    #region UpdateContactAsync Tests (2 tests)

    [Fact]
    public async Task UpdateContactAsync_WithValidData_UpdatesContact()
    {
        // Arrange
        var contact = await TestHelpers.SeedSingleContactAsync(_context, _userId);
        
        var dto = new UpdateContactDto(
            FirstName: "Updated",
            LastName: "Name",
            Email: "updated@example.com",
            PhoneNumber: "+1-999-888-7777",
            BirthDate: new DateOnly(1995, 10, 20),
            Address: "Updated Address",
            Notes: "Updated notes"
        );

        // Act
        var result = await _sut.UpdateContactAsync(_userId, contact.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("Name");
        result.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task UpdateContactAsync_WithOtherUsersContact_ReturnsNull()
    {
        // Arrange
        var otherUserId = Guid.NewGuid().ToString();
        var contact = await TestHelpers.SeedSingleContactAsync(_context, otherUserId);

        var dto = new UpdateContactDto(
            FirstName: "Hacker",
            LastName: "Attempt",
            Email: "hacker@evil.com",
            PhoneNumber: null,
            BirthDate: null,
            Address: null,
            Notes: null
        );

        // Act
        var result = await _sut.UpdateContactAsync(_userId, contact.Id, dto);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteContactAsync Tests (2 tests) - Soft Delete

    [Fact]
    public async Task DeleteContactAsync_WithValidId_SoftDeletesContact()
    {
        // Arrange
        var contact = await TestHelpers.SeedSingleContactAsync(_context, _userId);

        // Act
        var result = await _sut.DeleteContactAsync(_userId, contact.Id);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        var deletedContact = await _context.Contacts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == contact.Id);
        
        deletedContact.Should().NotBeNull();
        deletedContact!.IsDeleted.Should().BeTrue();
        deletedContact.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteContactAsync_WithOtherUsersContact_ReturnsFalse()
    {
        // Arrange
        var otherUserId = Guid.NewGuid().ToString();
        var contact = await TestHelpers.SeedSingleContactAsync(_context, otherUserId);

        // Act
        var result = await _sut.DeleteContactAsync(_userId, contact.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
