using ContactManagement.Api.Data;
using ContactManagement.Api.DTOs.Contacts;
using ContactManagement.Api.Models;
using ContactManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.Api.Services;

/// <summary>
/// Contact business logic service.
/// KEY POINT: Every method requires userId and filters data accordingly.
/// This ensures users can ONLY access their own contacts.
/// </summary>
public class ContactService : IContactService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ContactService> _logger;

    public ContactService(ApplicationDbContext context, ILogger<ContactService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResultDto<ContactDto>> GetContactsAsync(
        string userId,
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDescending = false,
        string? searchTerm = null)
    {
        // Start with user's contacts only - DATA ISOLATION!
        var query = _context.Contacts
            .AsNoTracking()
            .Where(c => c.UserId == userId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(term)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortDescending 
                ? query.OrderByDescending(c => c.LastName).ThenByDescending(c => c.FirstName)
                : query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName),
            "birthdate" => sortDescending
                ? query.OrderByDescending(c => c.BirthDate)
                : query.OrderBy(c => c.BirthDate),
            "email" => sortDescending
                ? query.OrderByDescending(c => c.Email)
                : query.OrderBy(c => c.Email),
            "createdat" => sortDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName) // Default sort
        };

        // Apply pagination
        var contacts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResultDto<ContactDto>(
            Items: contacts,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        );
    }

    public async Task<ContactDto?> GetContactByIdAsync(string userId, int contactId)
    {
        var contact = await _context.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        return contact == null ? null : MapToDto(contact);
    }

    public async Task<ContactDto> CreateContactAsync(string userId, CreateContactDto dto)
    {
        var contact = new Contact
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            BirthDate = dto.BirthDate,
            Address = dto.Address?.Trim(),
            Notes = dto.Notes?.Trim(),
            UserId = userId, // Associate with current user
            CreatedAt = DateTime.UtcNow
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Contact {ContactId} created for user {UserId}", 
            contact.Id, userId);

        return MapToDto(contact);
    }

    public async Task<ContactDto?> UpdateContactAsync(
        string userId, 
        int contactId, 
        UpdateContactDto dto)
    {
        // Find contact AND verify ownership
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null)
        {
            return null; // Contact doesn't exist OR doesn't belong to this user
        }

        // Update properties
        contact.FirstName = dto.FirstName.Trim();
        contact.LastName = dto.LastName.Trim();
        contact.Email = dto.Email.Trim().ToLower();
        contact.PhoneNumber = dto.PhoneNumber?.Trim();
        contact.BirthDate = dto.BirthDate;
        contact.Address = dto.Address?.Trim();
        contact.Notes = dto.Notes?.Trim();
        contact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Contact {ContactId} updated by user {UserId}", 
            contactId, userId);

        return MapToDto(contact);
    }

    public async Task<bool> DeleteContactAsync(string userId, int contactId)
    {
        // Find and verify ownership in one query
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null)
        {
            return false;
        }

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Contact {ContactId} deleted by user {UserId}", 
            contactId, userId);

        return true;
    }

    // Helper method to map Entity to DTO
    private static ContactDto MapToDto(Contact contact) => new(
        Id: contact.Id,
        FirstName: contact.FirstName,
        LastName: contact.LastName,
        Email: contact.Email,
        PhoneNumber: contact.PhoneNumber,
        BirthDate: contact.BirthDate,
        Address: contact.Address,
        Notes: contact.Notes,
        CreatedAt: contact.CreatedAt,
        UpdatedAt: contact.UpdatedAt
    );
}
