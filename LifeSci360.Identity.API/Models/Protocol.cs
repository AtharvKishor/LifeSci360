using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Protocol
    {
        [Key]
        public int ProtocolID { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Phase { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(450)]
        public string? CreatedByUserID { get; set; }

        // Stores phases as JSON — no new table needed
    

        // Navigation
        public ICollection<Site> Sites { get; set; } = new List<Site>();
    }
}