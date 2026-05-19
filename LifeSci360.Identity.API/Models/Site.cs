using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LifeSci360.Identity.API.Models
{
    public class Site
    {
        [Key]
        public Guid SiteID { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(300)]
        public string Location { get; set; } = null!;

        public Guid? InvestigatorID { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Active";

        public Guid ProtocolID { get; set; }

        public Guid? SampleID { get; set; }

        [ForeignKey("ProtocolID")]
        public Protocol? Protocol { get; set; }
    }
}
