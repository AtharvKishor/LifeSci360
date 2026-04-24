namespace LifeSci360.Shared.DTOs
{
    public class CreateProtocolRequest
    {
        public string Title { get; set; } = null!;
        public string Phase { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Draft";
        public string? Description { get; set; }
    }

    public class UpdateProtocolRequest
    {
        public string Title { get; set; } = null!;
        public string Phase { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class ProtocolDto
    {
        public Guid ProtocolID { get; set; }
        public string Title { get; set; } = null!;
        public string Phase { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedByUserID { get; set; }
        public List<SiteDto> Sites { get; set; } = new();
    }
}