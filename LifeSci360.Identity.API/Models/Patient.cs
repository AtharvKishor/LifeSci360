using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Patient
    {
        [Key]
        public Guid PatientID { get; set; }
        public Guid ProtocolID { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; } 
        public string? ContactInfo { get; set; }
        public string Status { get; set; }
        public DateTime EnrolledDate { get; set; }
    }
}