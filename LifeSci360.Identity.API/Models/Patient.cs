using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Patient
    {
        [Key]
        public int PatientID { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        public DateTime DOB { get; set; }

        [MaxLength(500)]
        public string? ContactInfo { get; set; }

        [Required, MaxLength(50)]
        public string EnrollmentStatus { get; set; } = "Pending";
    }
}