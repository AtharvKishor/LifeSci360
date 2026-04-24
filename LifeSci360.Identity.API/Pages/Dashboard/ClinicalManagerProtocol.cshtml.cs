using LifeSci360.Identity.API.Data;
using LifeSci360.Identity.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LifeSci360.Identity.API.Pages.Dashboard
{
    [Authorize(Roles = "Admin,ClinicalTrialManager")]
    public class ClinicalManagerModel : PageModel
    {
        private readonly AppIdentityDbContext _context;

        public ClinicalManagerModel(AppIdentityDbContext context)
        {
            _context = context;
        }

        //Page Data 
        public List<Protocol> Protocols { get; set; } = new();
        public List<User> Investigators { get; set; } = new();
        public List<string> AssignedInvestigatorIDs { get; set; } = new(); // string to match User.Id for UI comparisons
        public int TotalSites { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int CompletedCount { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public string TodayString { get; set; } =
            DateTime.UtcNow.ToString("yyyy-MM-dd");
        public string MaxDateString { get; set; } = "2035-12-31";

        //Create Protocol
        [BindProperty] public string Title { get; set; } = null!;
        [BindProperty] public string Phase { get; set; } = null!;
        [BindProperty] public DateTime StartDate { get; set; }
        [BindProperty] public DateTime EndDate { get; set; }
        [BindProperty] public string? Description { get; set; }

        //Edit Protocol
        [BindProperty] public Guid EditProtocolID { get; set; }
        [BindProperty] public string EditTitle { get; set; } = null!;
        [BindProperty] public string EditPhase { get; set; } = null!;
        [BindProperty] public DateTime EditStartDate { get; set; }
        [BindProperty] public DateTime EditEndDate { get; set; }
        [BindProperty] public string? EditDescription { get; set; }

        // Add Site
        [BindProperty] public string SiteName { get; set; } = null!;
        [BindProperty] public string SiteLocation { get; set; } = null!;
        [BindProperty] public Guid? SiteInvestigatorID { get; set; }
        [BindProperty] public Guid SiteProtocolID { get; set; }

        //Edit Site
        [BindProperty] public Guid EditSiteID { get; set; }
        [BindProperty] public string EditSiteName { get; set; } = null!;
        [BindProperty] public string EditSiteLocation { get; set; } = null!;
        [BindProperty] public string EditSiteStatus { get; set; } = null!;
        [BindProperty] public Guid? EditSiteInvestigatorID { get; set; }
        [BindProperty] public Guid EditSiteProtocolID { get; set; }

        //GET
        public async Task OnGetAsync()
        {
            await AutoUpdateStatusAsync();
            await LoadDataAsync();
        }

        //Auto update protocol status
        private async Task AutoUpdateStatusAsync()
        {
            var protocols = await _context.Protocols
                .Include(p => p.Sites)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            bool changed = false;

            foreach (var p in protocols)
            {
                string newStatus;
                if (today < p.StartDate.Date)
                    newStatus = "Inactive";
                else if (today <= p.EndDate.Date)
                    newStatus = "Active";
                else
                    newStatus = "Completed";

                if (p.Status != newStatus)
                {
                    p.Status = newStatus;

                    if (newStatus == "Completed")
                        foreach (var s in p.Sites
                            .Where(s => s.Status == "Active"))
                            s.Status = "Closed";

                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserID = "system",
                        Action = $"Auto-updated Protocol " +
                                    $"'{p.Title}' to {newStatus}",
                        Timestamp = DateTime.UtcNow
                    });
                    changed = true;
                }
            }

            if (changed)
                await _context.SaveChangesAsync();
        }

        //POST: Create Protocol
        public async Task<IActionResult> OnPostCreateProtocolAsync()
        {
            var today = DateTime.UtcNow.Date;

            if (string.IsNullOrEmpty(Title))
            {
                ErrorMessage = "Title is required.";
                await LoadDataAsync();
                return Page();
            }

         

            if (EndDate.Date <= StartDate.Date)
            {
                ErrorMessage =
                    "End date must be after the start date.";
                await LoadDataAsync();
                return Page();
            }

            string autoStatus = today < StartDate.Date
                ? "Inactive" : "Active";

            var userId = User.FindFirstValue(
                             ClaimTypes.NameIdentifier) ?? "system";

            var protocol = new Protocol
            {
                Title = Title,
                Phase = Phase,
                Status = autoStatus,
                StartDate = StartDate,
                EndDate = EndDate,
                Description = Description,
                CreatedDate = DateTime.UtcNow,
                CreatedByUserID = userId
            };

            _context.Protocols.Add(protocol);
            _context.AuditLogs.Add(new AuditLog
            {
                UserID = userId,
                Action = $"Created Protocol: {protocol.Title}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            SuccessMessage = $"Protocol '{Title}' created.";
            return RedirectToPage();
        }

        //POST: Edit Protocol
        public async Task<IActionResult> OnPostEditProtocolAsync()
        {
            var today = DateTime.UtcNow.Date;

            if (string.IsNullOrEmpty(EditTitle))
            {
                ErrorMessage = "Title is required.";
                await LoadDataAsync();
                return Page();
            }

          

            if (EditEndDate.Date <= EditStartDate.Date)
            {
                ErrorMessage =
                    "End date must be after the start date.";
                await LoadDataAsync();
                return Page();
            }

            var protocol = await _context.Protocols
                .FirstOrDefaultAsync(
                    p => p.ProtocolID == EditProtocolID);

            if (protocol == null)
            {
                ErrorMessage = "Protocol not found.";
                await LoadDataAsync();
                return Page();
            }

            protocol.Title = EditTitle;
            protocol.Phase = EditPhase;
            protocol.StartDate = EditStartDate;
            protocol.EndDate = EditEndDate;
            protocol.Description = EditDescription;

            if (today < EditStartDate.Date)
                protocol.Status = "Inactive";
            else if (today <= EditEndDate.Date)
                protocol.Status = "Active";
            else
                protocol.Status = "Completed";

            _context.AuditLogs.Add(new AuditLog
            {
                UserID = User.FindFirstValue(
                                ClaimTypes.NameIdentifier) ?? "system",
                Action = $"Updated Protocol: '{EditTitle}'",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            SuccessMessage = $"Protocol '{EditTitle}' updated.";
            return RedirectToPage();
        }

        // POST: Add Site 

        // ── POST: Add Site ─────────────────────────────────────
        public async Task<IActionResult> OnPostAddSiteAsync()
        {
            var protocol = await _context.Protocols
                .FindAsync(SiteProtocolID);

            if (protocol == null)
            {
                ErrorMessage = "Protocol not found.";
                await LoadDataAsync();
                return Page();
            }

            if (string.IsNullOrEmpty(SiteName) ||
                string.IsNullOrEmpty(SiteLocation))
            {
                ErrorMessage = "Site name and location are required.";
                await LoadDataAsync();
                return Page();
            }

            if (SiteInvestigatorID.HasValue)
            {
                var alreadyAssigned = await _context.Sites
                    .AnyAsync(s => s.InvestigatorID == SiteInvestigatorID);

                if (alreadyAssigned)
                {
                    ErrorMessage =
                        "This investigator is already assigned " +
                        "to another site.";
                    await LoadDataAsync();
                    return Page();
                }
            }

            var site = new Site
            {
                Name = SiteName,
                Location = SiteLocation,
                Status = protocol.Status == "Completed"
                                 ? "Closed" : "Active",
                InvestigatorID = SiteInvestigatorID,
                ProtocolID = SiteProtocolID
            };

            _context.Sites.Add(site);
            _context.AuditLogs.Add(new AuditLog
            {
                UserID = User.FindFirstValue(
                                ClaimTypes.NameIdentifier) ?? "system",
                Action = $"Added Site '{SiteName}' " +
                            $"to Protocol ID: {SiteProtocolID}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            SuccessMessage = $"Site '{SiteName}' added.";
            return RedirectToPage();
        }

        //POST: Edit Site
        public async Task<IActionResult> OnPostEditSiteAsync()
        {
            var site = await _context.Sites
                .Include(s => s.Protocol)
                .FirstOrDefaultAsync(s => s.SiteID == EditSiteID);

            if (site == null)
            {
                ErrorMessage = "Site not found.";
                await LoadDataAsync();
                return Page();
            }

            if (string.IsNullOrEmpty(EditSiteName) ||
                string.IsNullOrEmpty(EditSiteLocation))
            {
                ErrorMessage =
                    "Site name and location are required.";
                await LoadDataAsync();
                return Page();
            }

            if (EditSiteInvestigatorID.HasValue)
            {
                var alreadyAssigned = await _context.Sites
                    .AnyAsync(s =>
                        s.InvestigatorID == EditSiteInvestigatorID
                        && s.SiteID != EditSiteID);

                if (alreadyAssigned)
                {
                    ErrorMessage =
                        "This investigator is already assigned " +
                        "to another site.";
                    await LoadDataAsync();
                    return Page();
                }
            }

            site.Name = EditSiteName;
            site.Location = EditSiteLocation;
            site.Status = EditSiteStatus;
            site.InvestigatorID = EditSiteInvestigatorID;

            _context.AuditLogs.Add(new AuditLog
            {
                UserID = User.FindFirstValue(
                                ClaimTypes.NameIdentifier) ?? "system",
                Action = $"Updated Site '{EditSiteName}'",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            SuccessMessage = $"Site '{EditSiteName}' updated.";
            return RedirectToPage();
        }

        //POST: Delete Site
        public async Task<IActionResult> OnPostDeleteSiteAsync(
            Guid siteId)
        {
            var site = await _context.Sites
                .FirstOrDefaultAsync(s => s.SiteID == siteId);

            if (site != null)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserID = User.FindFirstValue(
                                    ClaimTypes.NameIdentifier) ?? "system",
                    Action = $"Deleted Site '{site.Name}'",
                    Timestamp = DateTime.UtcNow
                });
                _context.Sites.Remove(site);
                await _context.SaveChangesAsync();
                SuccessMessage = $"Site '{site.Name}' deleted.";
            }

            return RedirectToPage();
        }

        //POST: Delete Protocol
        public async Task<IActionResult> OnPostDeleteProtocolAsync(
            Guid protocolId)
        {
            var protocol = await _context.Protocols
                .Include(p => p.Sites)
                .FirstOrDefaultAsync(
                    p => p.ProtocolID == protocolId);

            if (protocol != null)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserID = User.FindFirstValue(
                                    ClaimTypes.NameIdentifier) ?? "system",
                    Action = $"Deleted Protocol: {protocol.Title}",
                    Timestamp = DateTime.UtcNow
                });
                _context.Protocols.Remove(protocol);
                await _context.SaveChangesAsync();
                SuccessMessage = "Protocol deleted.";
            }

            return RedirectToPage();
        }

        //  Load Data 
        private async Task LoadDataAsync()
        {
            Protocols = await _context.Protocols
                .Include(p => p.Sites)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            Investigators = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            AssignedInvestigatorIDs = await _context.Sites
                .Where(s => s.InvestigatorID != null)
                .Select(s => s.InvestigatorID!.Value.ToString())
                .ToListAsync();

            TotalSites = Protocols.Sum(p => p.Sites.Count);
            ActiveCount = Protocols.Count(p => p.Status == "Active");
            InactiveCount = Protocols.Count(p => p.Status == "Inactive");
            CompletedCount = Protocols.Count(p => p.Status == "Completed");
        }

        //Helpers 
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

        public string GetInvestigatorName(Guid? id) =>
            id == null ? "Not assigned"
            : Investigators.FirstOrDefault(u => u.Id == id.Value.ToString())?.FullName
              ?? "Unknown";
    }
}