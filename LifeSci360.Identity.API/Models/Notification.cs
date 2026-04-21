using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Notification
    {
        [Key]

        public Guid NotificationID { get; set; }
        public string Message { get; set; }
        public string Channel { get; set; }
        public string RecipientEmail { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
    }

}
