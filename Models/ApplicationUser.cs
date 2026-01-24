using Microsoft.AspNetCore.Identity;

namespace JobPortal_ServerSide.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public DeveloperProfile DeveloperProfile { get; set; }
    }
}
