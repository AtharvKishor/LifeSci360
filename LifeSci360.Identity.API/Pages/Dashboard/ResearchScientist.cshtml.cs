using LifeSci360.Identity.API.Data;
using LifeSci360.Identity.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LifeSci360.Identity.API.Pages.Dashboard
{
    [Authorize(Roles = "Admin,ResearchScientist")]
    public class ResearchScientistModel : PageModel
    {
        private readonly AppIdentityDbContext _context;

        public ResearchScientistModel(AppIdentityDbContext context)
        {
            _context = context;
        }

        //  Page Data 
        public List<Protocol> Protocols { get; set; } = new();
        public int TotalSites { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int CompletedCount { get; set; }

        //  GET 
        public async Task OnGetAsync()
        {
            Protocols = await _context.Protocols
                .Include(p => p.Sites)
                    .ThenInclude(s => s.Investigator)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            TotalSites = Protocols.Sum(p => p.Sites.Count);
            ActiveCount = Protocols.Count(p => p.Status == "Active");
            InactiveCount = Protocols.Count(p => p.Status == "Inactive");
            CompletedCount = Protocols.Count(p => p.Status == "Completed");
        }

        //  Helpers 
        public string GetProtocolBadge(string status) => status switch
        {
            "Active" => "text-success bg-success bg-opacity-10",
            "Inactive" => "text-warning bg-warning bg-opacity-10",
            "Completed" => "text-secondary bg-secondary bg-opacity-10",
            _ => "text-info bg-info bg-opacity-10"
        };

        public string GetSiteBadge(string status) => status switch
        {
            "Active" => "text-success bg-success bg-opacity-10",
            "Inactive" => "text-warning bg-warning bg-opacity-10",
            "Closed" => "text-danger bg-danger bg-opacity-10",
            _ => "text-secondary bg-secondary bg-opacity-10"
        };

        public string GetInitial(string name) =>
            string.IsNullOrEmpty(name) ? "?"
            : name.Substring(0, 1).ToUpper();
    }
}