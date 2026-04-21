using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Patient
    {
        [Key]
        public Guid PatientID { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        [MaxLength(500)]
        public string? ContactInfo { get; set; }

        public DateTime EnrolledDate { get; set; }

        [Required, MaxLength(50)]
        public string EnrollmentStatus { get; set; } = "Pending";
    }
}