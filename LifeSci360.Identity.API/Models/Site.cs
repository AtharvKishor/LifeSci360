using System.ComponentModel.DataAnnotations;

namespace LifeSci360.Identity.API.Models
{
    public class Site
    {
        [Key]

        public Guid SiteID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public Guid InvestigatorID { get; set; }
        public string Status { get; set; }
    }
}
