namespace LifeSci360.Shared.DTOs
{
    public class CreateSiteRequest
    {
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string? InvestigatorID { get; set; }
        public int ProtocolID { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class UpdateSiteRequest
    {
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string? InvestigatorID { get; set; }
        public string Status { get; set; } = null!;
    }

    public class SiteDto
    {
        public int SiteID { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string? InvestigatorID { get; set; }
        public string Status { get; set; } = null!;
        public int ProtocolID { get; set; }
    }
    public class InvestigatorDto
    {
        public string ID { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}