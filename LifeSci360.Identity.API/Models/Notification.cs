using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        [MaxLength(450)]
        public string? UserID { get; set; }

        [Required, MaxLength(1000)]
        public string Message { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Category { get; set; } = null!;   

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Unread";  

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}