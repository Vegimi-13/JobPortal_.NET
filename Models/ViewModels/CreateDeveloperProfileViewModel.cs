using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JobPortal_ServerSide.Models.ViewModels
{
    public class CreateDeveloperProfileViewModel
    {
        [Required]
        public string? FullName { get; set; }

        [Required]
        public string? Title { get; set; }

        public string? Bio { get; set; }

        public string? Location { get; set; }

        [Required]
        [Display(Name = "Skills")]
        public List<int> SkillIds { get; set; } = new();

        public IEnumerable<SelectListItem> Skills { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
