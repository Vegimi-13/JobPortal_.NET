namespace JobPortal_ServerSide.Models
{
    public class Project
    {
        public int Id { get; set; }

        public int DeveloperProfileId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }

        public DeveloperProfile DeveloperProfile { get; set; }
    }
}
