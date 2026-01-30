using ContactManagement.Api.DTOs.Contacts;
using ContactManagement.Api.Extensions;
using ContactManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService)
    {
        _contactService = contactService;
    }


    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await _contactService.GetContactsAsync(
            User.GetUserId(), page, pageSize, sortBy, sortDescending, search);

        return Ok(result);
    }


    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContact(int id)
    {
        var contact = await _contactService.GetContactByIdAsync(User.GetUserId(), id);

        if (contact == null)
        {
            return NotFound(new { Message = "Contact not found" });
        }

        return Ok(contact);
    }


    [HttpPost]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactDto dto)
    {
        var contact = await _contactService.CreateContactAsync(User.GetUserId(), dto);

        return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContact(int id, [FromBody] UpdateContactDto dto)
    {
        var contact = await _contactService.UpdateContactAsync(User.GetUserId(), id, dto);

        if (contact == null)
        {
            return NotFound(new { Message = "Contact not found" });
        }

        return Ok(contact);
    }


    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var deleted = await _contactService.DeleteContactAsync(User.GetUserId(), id);

        if (!deleted)
        {
            return NotFound(new { Message = "Contact not found" });
        }

        return NoContent();
    }
}