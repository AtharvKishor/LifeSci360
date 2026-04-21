using LifeSci360.Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LifeSci360.Identity.API.Data
{
    public class AppIdentityDbContext
        : IdentityDbContext<User, ApplicationRole, string>
    {
        public AppIdentityDbContext(
            DbContextOptions<AppIdentityDbContext> options)
            : base(options)
        {
        }

        // ── DbSets ─────────────────────────────────────────────
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Protocol> Protocols { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Sample> Samples { get; set; }
        public DbSet<LabResult> LabResults { get; set; }
        public DbSet<ComplianceReport> ComplianceReports { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Identity Tables ────────────────────────────────
            builder.Entity<User>()
                   .ToTable("Users");
            builder.Entity<ApplicationRole>()
                   .ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>()
                   .ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>()
                   .ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>()
                   .ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>()
                   .ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<string>>()
                   .ToTable("RoleClaims");

            // ── AuditLog ───────────────────────────────────────
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.AuditID);
                entity.ToTable("AuditLogs");
                entity.Property(a => a.AuditID)
                      .ValueGeneratedOnAdd();
                entity.Property(a => a.UserID)
                      .IsRequired();
                entity.Property(a => a.Action)
                      .IsRequired()
                      .HasMaxLength(255);
                entity.Property(a => a.Timestamp)
                      .IsRequired();
            });

            // ── Protocol ───────────────────────────────────────
            builder.Entity<Protocol>(entity =>
            {
                entity.HasKey(p => p.ProtocolID);
                entity.ToTable("Protocols");
                entity.Property(p => p.ProtocolID)
                      .ValueGeneratedOnAdd();
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
                      .ValueGeneratedOnAdd();
                entity.Property(s => s.Name)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(s => s.Location)
                      .IsRequired()
                      .HasMaxLength(300);
                entity.Property(s => s.Status)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(s => s.InvestigatorID)
                      .HasMaxLength(450);

                // Site → Protocol
                entity.HasOne(s => s.Protocol)
                      .WithMany(p => p.Sites)
                      .HasForeignKey(s => s.ProtocolID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Site → User (Investigator)
                entity.HasOne(s => s.Investigator)
                      .WithMany()
                      .HasForeignKey(s => s.InvestigatorID)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            // ── Patient ────────────────────────────────────────
            builder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.PatientID);
                entity.ToTable("Patients");
                entity.Property(p => p.PatientID)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");
                entity.Property(p => p.FullName)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(p => p.ContactInfo)
                      .HasMaxLength(500);
                entity.Property(p => p.EnrollmentStatus)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            // ── Visit ──────────────────────────────────────────
            builder.Entity<Visit>(entity =>
            {
                entity.HasKey(v => v.VisitID);
                entity.ToTable("Visits");
                entity.Property(v => v.VisitID)
                      .ValueGeneratedOnAdd();
                entity.Property(v => v.Status)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(v => v.Notes)
                      .HasMaxLength(1000);

                // Visit → Patient
                entity.HasOne<Patient>()
                      .WithMany()
                      .HasForeignKey(v => v.PatientID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Visit → Protocol
                entity.HasOne<Protocol>()
                      .WithMany()
                      .HasForeignKey(v => v.ProtocolID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Sample ─────────────────────────────────────────
            builder.Entity<Sample>(entity =>
            {
                entity.HasKey(s => s.SampleID);
                entity.ToTable("Samples");
                entity.Property(s => s.SampleID)
                      .ValueGeneratedOnAdd();
                entity.Property(s => s.Status)
                      .IsRequired()
                      .HasMaxLength(50);

                // Sample → Protocol
                entity.HasOne<Protocol>()
                      .WithMany()
                      .HasForeignKey(s => s.ProtocolID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Sample → Patient
                entity.HasOne<Patient>()
                      .WithMany()
                      .HasForeignKey(s => s.PatientID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── LabResult ──────────────────────────────────────
            builder.Entity<LabResult>(entity =>
            {
                entity.HasKey(l => l.ResultID);
                entity.ToTable("LabResults");
                entity.Property(l => l.ResultID)
                      .ValueGeneratedOnAdd();
                entity.Property(l => l.TestType)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(l => l.ResultValue)
                      .IsRequired()
                      .HasMaxLength(500);
                entity.Property(l => l.Status)
                      .IsRequired()
                      .HasMaxLength(50);

                // LabResult → Sample
                entity.HasOne<Sample>()
                      .WithMany()
                      .HasForeignKey(l => l.SampleID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── ComplianceReport ───────────────────────────────
            builder.Entity<ComplianceReport>(entity =>
            {
                entity.HasKey(c => c.ReportID);
                entity.ToTable("ComplianceReports");
                entity.Property(c => c.ReportID)
                      .ValueGeneratedOnAdd();
                entity.Property(c => c.Scope)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(c => c.Metrics)
                      .HasMaxLength(2000);
                entity.Property(c => c.GeneratedDate)
                      .IsRequired();
            });

            // ── Notification ───────────────────────────────────
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.NotificationID);
                entity.ToTable("Notifications");
                entity.Property(n => n.NotificationID)
                      .ValueGeneratedOnAdd();
                entity.Property(n => n.Message)
                      .IsRequired()
                      .HasMaxLength(1000);
                entity.Property(n => n.Category)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(n => n.Status)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(n => n.CreatedDate)
                      .IsRequired();

                // Notification → User
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(n => n.UserID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}