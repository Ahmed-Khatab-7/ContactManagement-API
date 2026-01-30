using Microsoft.AspNetCore.Identity;

namespace ContactManagement.Api.Models;

/// <summary>
/// Extended Identity user with additional profile fields.
/// Why extend IdentityUser? To add custom properties while leveraging 
/// built-in password hashing, security stamps, and lockout features.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property - One user has many contacts
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
