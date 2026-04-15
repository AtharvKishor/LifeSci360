using LifeSci360.Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace LifeSci360.Identity.API.Data
{
    public class AppIdentityDbContext : IdentityDbContext<User, ApplicationRole, string>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }

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
        }
    }
}