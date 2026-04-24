using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Protocol
    {
        [Key]
        public Guid ProtocolID { get; set; }

        public string Title { get; set; } = null!;

        public string Phase { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? CreatedByUserID { get; set; }

        public ICollection<Site> Sites { get; set; } = [];
    }
}
