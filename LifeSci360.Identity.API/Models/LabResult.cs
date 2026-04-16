using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class LabResult
    {
        [Key]

        public Guid ResultID { get; set; }
        public Guid SampleID { get; set; }
        public string TestType { get; set; }
        public string ResultValue { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}
