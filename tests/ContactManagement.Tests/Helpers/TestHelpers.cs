using ContactManagement.Api.Data;
using ContactManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContactManagement.Tests.Helpers;

public static class TestHelpers
{
    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    public static ApplicationDbContext CreateInMemoryDbContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        
        return context;
    }

    /// <summary>
    /// Creates a mock logger
    /// </summary>
    public static ILogger<T> CreateMockLogger<T>()
    {
        return Mock.Of<ILogger<T>>();
    }

    /// <summary>
    /// Creates a test user
    /// </summary>
    public static ApplicationUser CreateTestUser(string? id = null, string? email = null)
    {
        return new ApplicationUser
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Email = email ?? "test@example.com",
            UserName = email ?? "test@example.com",
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test contact using factory method
    /// </summary>
    public static Contact CreateTestContact(string userId, string? email = null)
    {
        return Contact.Create(
            firstName: "John",
            lastName: "Doe",
            email: email ?? $"john{Guid.NewGuid():N}@example.com",
            userId: userId,
            phoneNumber: "+1-555-123-4567",
            birthDate: new DateOnly(1990, 5, 15),
            address: "123 Test Street",
            notes: "Test contact"
        );
    }

    /// <summary>
    /// Seeds multiple contacts for a user
    /// </summary>
    public static async Task<List<Contact>> SeedContactsAsync(
        ApplicationDbContext context, 
        string userId, 
        int count = 5)
    {
        var contacts = new List<Contact>();
        
        for (int i = 1; i <= count; i++)
        {
            var contact = Contact.Create(
                firstName: $"FirstName{i}",
                lastName: $"LastName{i}",
                email: $"contact{i}_{Guid.NewGuid():N}@example.com",
                userId: userId,
                phoneNumber: $"+1-555-000-{i:D4}",
                birthDate: new DateOnly(1990 + i, 1, 1)
            );
            
            contacts.Add(contact);
        }

        context.Contacts.AddRange(contacts);
        await context.SaveChangesAsync();

        return contacts;
    }

    /// <summary>
    /// Seeds a single contact and returns it
    /// </summary>
    public static async Task<Contact> SeedSingleContactAsync(
        ApplicationDbContext context,
        string userId,
        string firstName = "Test",
        string lastName = "Contact")
    {
        var contact = Contact.Create(
            firstName: firstName,
            lastName: lastName,
            email: $"{firstName.ToLower()}.{lastName.ToLower()}_{Guid.NewGuid():N}@example.com",
            userId: userId,
            phoneNumber: "+1-555-999-9999"
        );

        context.Contacts.Add(contact);
        await context.SaveChangesAsync();

        return contact;
    }
}
