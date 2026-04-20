namespace LifeSci360.Shared.DTOs
{
    public class VisitDto
    {
        public Guid VisitID { get; set; }
        public Guid PatientID { get; set; }
        public Guid ProtocolID { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}