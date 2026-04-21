using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Visit
    {
        [Key]
        public Guid VisitID { get; set; }
        public Guid PatientID { get; set; }
        public Guid ProtocolID { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
    }
}