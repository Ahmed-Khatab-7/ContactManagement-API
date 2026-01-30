using System.Security.Claims;
using ContactManagement.Api.DTOs.Contacts;
using ContactManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService)
    {
        _contactService = contactService;
    }

    /// <summary>
    /// Gets the current user's ID from the JWT token claims.
    /// This is how we implement data isolation at the controller level.
    /// </summary>
    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        
        return userId;
    }

    /// <summary>
    /// Get all contacts for the current user with pagination and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? search = null)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Prevent excessive page sizes
        
        var userId = GetCurrentUserId();
        var result = await _contactService.GetContactsAsync(
            userId, page, pageSize, sortBy, sortDescending, search);
        
        return Ok(result);
    }

    /// <summary>
    /// Get a specific contact by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContact(int id)
    {
        var userId = GetCurrentUserId();
        var contact = await _contactService.GetContactByIdAsync(userId, id);
        
        if (contact == null)
        {
            return NotFound(new { Message = "Contact not found" });
        }
        
        return Ok(contact);
    }

    /// <summary>
    /// Create a new contact.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactDto dto)
    {
        var userId = GetCurrentUserId();
        var contact = await _contactService.CreateContactAsync(userId, dto);
        
        return CreatedAtAction(
            nameof(GetContact), 
            new { id = contact.Id }, 
            contact);
    }

    /// <summary>
    /// Update an existing contact.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContact(int id, [FromBody] UpdateContactDto dto)
    {
        var userId = GetCurrentUserId();
        var contact = await _contactService.UpdateContactAsync(userId, id, dto);
        
        if (contact == null)
        {
            return NotFound(new { Message = "Contact not found" });
        }
        
        return Ok(contact);
    }

    /// <summary>
    /// Delete a contact.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var userId = GetCurrentUserId();
        var deleted = await _contactService.DeleteContactAsync(userId, id);
        
        if (!deleted)
        {
            return NotFound(new { Message = "Contact not found" });
        }
        
        return NoContent();
    }
}
