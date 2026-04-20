using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Visit
    {
        [Key]
        public int VisitID { get; set; }

        public int PatientID { get; set; }

        public int ProtocolID { get; set; }    

        public Guid VisitID { get; set; }
        public Guid PatientID { get; set; }
        public Guid ProtocolID { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
    }
}
        public DateTime VisitDate { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = null!;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}