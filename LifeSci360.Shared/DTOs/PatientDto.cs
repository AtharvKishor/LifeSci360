using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Shared.DTOs
{
    public class PatientDto
    {
        public Guid PatientID { get; set; }
        public Guid ProtocolID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        [MaxLength(10)]
        public string? ContactInfo { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolledDate { get; set; }
    }
}