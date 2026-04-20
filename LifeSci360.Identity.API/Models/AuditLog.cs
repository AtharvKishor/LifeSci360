using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditID { get; set; }

        public string UserID { get; set; } = null!;

        public string Action { get; set; } = null!;

        public DateTime Timestamp { get; set; }
    }
}