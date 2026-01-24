namespace JobPortal_ServerSide.Models
{
    public class DeveloperSkill
    {
        public int DeveloperProfileId { get; set; }
        public DeveloperProfile DeveloperProfile { get; set; }

        public int SkillId { get; set; }
        public Skill Skill { get; set; }
    }
}
