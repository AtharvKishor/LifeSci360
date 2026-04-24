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
        public DbSet<Protocol> Protocols { get; set; }
        public DbSet<Sample> Samples { get; set; }

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

            // ── Protocol ───────────────────────────────────────
            builder.Entity<Protocol>(entity =>
            {
                entity.HasKey(p => p.ProtocolID);
                entity.ToTable("Protocols");
                entity.Property(p => p.ProtocolID)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");
                entity.Property(p => p.Title)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(p => p.Phase)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(p => p.StartDate)
                      .IsRequired();
                entity.Property(p => p.EndDate)
                      .IsRequired();
                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(p => p.Description)
                      .HasMaxLength(1000);
                entity.Property(p => p.CreatedDate)
                      .IsRequired();
                entity.Property(p => p.CreatedByUserID)
                      .HasMaxLength(450);

                // PhasesJson — stores phases as JSON string
              
            });

            // ── Site ───────────────────────────────────────────
            builder.Entity<Site>(entity =>
            {
                entity.HasKey(s => s.SiteID);
                entity.ToTable("Sites");
                entity.Property(s => s.SiteID)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");
                entity.Property(s => s.Name)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(s => s.Location)
                      .IsRequired()
                      .HasMaxLength(300);
                entity.Property(s => s.Status)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(s => s.InvestigatorID);
                entity.Property(s => s.SampleID);

                // Site → Protocol
                entity.HasOne(s => s.Protocol)
                      .WithMany(p => p.Sites)
                      .HasForeignKey(s => s.ProtocolID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Patient ────────────────────────────────────────
            builder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.PatientID);
                entity.ToTable("Patients");
                entity.Property(p => p.PatientID).ValueGeneratedOnAdd();
                entity.Property(p => p.FullName).IsRequired().HasMaxLength(255);
                entity.Property(p => p.ContactInfo).HasMaxLength(255);
                entity.Property(p => p.Status).IsRequired().HasMaxLength(50);
                entity.Property(p => p.EnrolledDate).IsRequired();
                entity.Property(p => p.PatientID)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");
                entity.Property(p => p.FullName)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(p => p.ContactInfo)
                      .HasMaxLength(500);
                entity.Property(p => p.ProtocolId);
                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasMaxLength(50);
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

                entity.ToTable("Samples");
                entity.Property(s => s.SampleID)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");
                entity.Property(s => s.Status)
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
