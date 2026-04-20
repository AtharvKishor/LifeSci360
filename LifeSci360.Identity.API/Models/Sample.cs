using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Sample
    {
        [Key]
        public int SampleID { get; set; }

        public int ProtocolID { get; set; }

        public int PatientID { get; set; }

        public DateTime CollectedDate { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = null!;
    }
}