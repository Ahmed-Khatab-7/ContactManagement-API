using ContactManagement.Api.DTOs.Contacts;

namespace ContactManagement.Api.Services.Interfaces;

public interface IContactService
{
    Task<PagedResultDto<ContactDto>> GetContactsAsync(
        string userId,
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDescending = false,
        string? searchTerm = null);
    
    Task<ContactDto?> GetContactByIdAsync(string userId, int contactId);
    Task<ContactDto> CreateContactAsync(string userId, CreateContactDto dto);
    Task<ContactDto?> UpdateContactAsync(string userId, int contactId, UpdateContactDto dto);
    Task<bool> DeleteContactAsync(string userId, int contactId);
}
