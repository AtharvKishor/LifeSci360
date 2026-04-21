namespace LifeSci360.Identity.API.Models
{
    public class AuditLog
    {
        public int AuditID { get; set; }
        public string UserID { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
