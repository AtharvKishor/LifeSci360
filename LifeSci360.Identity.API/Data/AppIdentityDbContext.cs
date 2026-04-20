using LifeSci360.Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LifeSci360.Identity.API.Data
{
    public class AppIdentityDbContext
        : IdentityDbContext<User, ApplicationRole, string>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }

        // ✅ Add Protocol DbSet
        public DbSet<Protocol> Protocols { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename Identity tables — remove AspNet prefix
            builder.Entity<User>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.AuditID);
                entity.ToTable("AuditLogs");
                entity.Property(a => a.AuditID).ValueGeneratedOnAdd();
                entity.Property(a => a.UserID).IsRequired();
                entity.Property(a => a.Action).IsRequired().HasMaxLength(255);
                entity.Property(a => a.Timestamp).IsRequired();
            });

            builder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.PatientID);
                entity.ToTable("Patients");
                entity.Property(p => p.PatientID).ValueGeneratedOnAdd();
                entity.Property(p => p.FullName).IsRequired().HasMaxLength(255);
                entity.Property(p => p.ContactInfo).HasMaxLength(255);
                entity.Property(p => p.Status).IsRequired().HasMaxLength(50);
                entity.Property(p => p.EnrolledDate).IsRequired();
            });

            builder.Entity<Visit>(entity =>
            {
                entity.HasKey(v => v.VisitID);
                entity.ToTable("Visits");
                entity.Property(v => v.VisitID).ValueGeneratedOnAdd();
                entity.Property(v => v.PatientID).IsRequired();
                entity.Property(v => v.ProtocolID).IsRequired();
                entity.Property(v => v.ScheduledDate).IsRequired();
                entity.Property(v => v.Status).IsRequired().HasMaxLength(50);
                entity.Property(v => v.Notes).HasMaxLength(500);
            });

            // ✅ Protocol configuration
            builder.Entity<Protocol>(entity =>
            {
                entity.HasKey(p => p.ProtocolID);
                entity.ToTable("Protocols");

                entity.Property(p => p.ProtocolID)
                      .ValueGeneratedOnAdd();

                entity.Property(p => p.Title)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(p => p.Phase)
                      .HasMaxLength(50);

                entity.Property(p => p.StartDate)
                      .IsRequired();

                entity.Property(p => p.EndDate)
                      .IsRequired();

                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasMaxLength(50);
            });
        }
    }
}
