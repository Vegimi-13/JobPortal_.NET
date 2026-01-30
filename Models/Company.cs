using System.ComponentModel.DataAnnotations;

namespace JobPortal_ServerSide.Models
{
    public class Company
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;

        public ApplicationUser? User { get; set; }
    }
}
