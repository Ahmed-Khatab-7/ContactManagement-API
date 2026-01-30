using Microsoft.AspNetCore.Identity;

namespace ContactManagement.Api.Models;


public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
