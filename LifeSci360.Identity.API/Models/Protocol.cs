using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Protocol
    {
        [Key]

        public Guid ProtocolID { get; set; }
        public string Title { get; set; }
        public string Phase { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
