using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class LabResult
    {
        [Key]
        public int ResultID { get; set; }

        public int SampleID { get; set; }

        [Required, MaxLength(100)]
        public string TestType { get; set; } = null!;

        [Required, MaxLength(500)]
        public string ResultValue { get; set; } = null!;

        public DateTime Date { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = null!;
    }
}