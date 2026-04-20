using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LifeSci360.Identity.API.Models
{
    public class Site
    {
        [Key]
        public int SiteID { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(300)]
        public string Location { get; set; } = null!;

        // string because ASP.NET Identity user IDs are strings
        [MaxLength(450)]
        public string? InvestigatorID { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Active";

        // FK to Protocol — was completely missing before
        public int ProtocolID { get; set; }

        // Navigation properties
        [ForeignKey("ProtocolID")]
        public Protocol? Protocol { get; set; }

        [ForeignKey("InvestigatorID")]
        public User? Investigator { get; set; }
    }
}