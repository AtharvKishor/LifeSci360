using Microsoft.AspNetCore.Identity;

namespace LifeSci360.Identity.API.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}