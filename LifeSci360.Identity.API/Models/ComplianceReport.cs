using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class ComplianceReport
    {
        [Key]
        public int ReportID { get; set; }

        [Required, MaxLength(200)]
        public string Scope { get; set; } = null!;

        [MaxLength(2000)]
        public string? Metrics { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    }
}