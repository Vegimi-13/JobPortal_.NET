namespace JobPortal_ServerSide.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<DeveloperSkill> DeveloperSkills { get; set; }
    }
}
