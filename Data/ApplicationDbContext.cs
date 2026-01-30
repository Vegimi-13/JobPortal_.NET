using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JobPortal_ServerSide.Models;

namespace JobPortal_ServerSide.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<DeveloperProfile> DeveloperProfiles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<DeveloperSkill> DeveloperSkills { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DeveloperSkill>()
                .HasKey(ds => new { ds.DeveloperProfileId, ds.SkillId });

            builder.Entity<DeveloperSkill>()
                .HasOne(ds => ds.DeveloperProfile)
                .WithMany(dp => dp.DeveloperSkills)
                .HasForeignKey(ds => ds.DeveloperProfileId);

            builder.Entity<DeveloperSkill>()
                .HasOne(ds => ds.Skill)
                .WithMany(s => s.DeveloperSkills)
                .HasForeignKey(ds => ds.SkillId);
        }
    }
}
