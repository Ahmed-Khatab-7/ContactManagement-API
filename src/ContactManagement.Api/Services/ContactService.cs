using ContactManagement.Api.Data;
using ContactManagement.Api.DTOs.Contacts;
using ContactManagement.Api.Models;
using ContactManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.Api.Services;

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

        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDescending);

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
        var contact = Contact.Create(
            firstName: dto.FirstName,
            lastName: dto.LastName,
            email: dto.Email,
            userId: userId,
            phoneNumber: dto.PhoneNumber,
            birthDate: dto.BirthDate,
            address: dto.Address,
            notes: dto.Notes
        );

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Contact {ContactId} created for user {UserId}", contact.Id, userId);

        return MapToDto(contact);
    }

    public async Task<ContactDto?> UpdateContactAsync(string userId, int contactId, UpdateContactDto dto)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null)
        {
            return null;
        }

        contact.Update(
            firstName: dto.FirstName,
            lastName: dto.LastName,
            email: dto.Email,
            phoneNumber: dto.PhoneNumber,
            birthDate: dto.BirthDate,
            address: dto.Address,
            notes: dto.Notes
        );

        await _context.SaveChangesAsync();

        _logger.LogInformation("Contact {ContactId} updated by user {UserId}", contactId, userId);

        return MapToDto(contact);
    }

    public async Task<bool> DeleteContactAsync(string userId, int contactId)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null)
        {
            return false;
        }

        // Soft delete
        contact.Delete();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Contact {ContactId} soft deleted by user {UserId}", contactId, userId);

        return true;
    }

    #region Private Methods

    private static IQueryable<Contact> ApplySorting(
        IQueryable<Contact> query,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.ToLower() switch
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
            _ => query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
        };
    }

    private static ContactDto MapToDto(Contact contact) => new(
        Id: contact.Id,
        FirstName: contact.FirstName,
        LastName: contact.LastName,
        Email: contact.Email,
        PhoneNumber: contact.PhoneNumber,
        BirthDate: contact.BirthDate,
        Address: contact.Address,
        Notes: contact.Notes
    );

    #endregion
}