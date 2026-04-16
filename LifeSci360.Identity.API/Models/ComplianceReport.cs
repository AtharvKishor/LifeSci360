using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class ComplianceReport
    {
        [Key]
        public Guid ReportID { get; set; }
        public string Title { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public string Status { get; set; }
    }
}
