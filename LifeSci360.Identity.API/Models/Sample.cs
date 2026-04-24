using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Sample
    {
        [Key]
        public Guid SampleID { get; set; }

        public Guid ProtocolID { get; set; }

        public Guid PatientID { get; set; }

        public Guid SampleID { get; set; }
        public Guid ProtocolID { get; set; }
        public Guid PatientID { get; set; }
        public DateTime CollectedDate { get; set; }
        public string Status { get; set; }
    }
}
