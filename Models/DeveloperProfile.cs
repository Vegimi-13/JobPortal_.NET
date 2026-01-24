namespace JobPortal_ServerSide.Models
{
    public class DeveloperProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string FullName { get; set; }
        public string Title { get; set; }
        public string Bio { get; set; }
        public string Location { get; set; }

        public bool IsActive { get; set; } = true;

        public ApplicationUser User { get; set; }
        public ICollection<Project> Projects { get; set; }
        public ICollection<DeveloperSkill> DeveloperSkills { get; set; }
    }
}
