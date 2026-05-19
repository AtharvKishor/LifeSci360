using LifeSci360.Shared.DTOs;
using LifeSci360.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LifeSci360.Identity.API.Pages.Dashboard
{
    [Authorize(Roles = "Admin,ClinicalTrialManager")]
    public class ClinicalManagerModels : PageModel
    {
        private readonly IProtocolService _protocolService;

        public ClinicalManagerModels(IProtocolService protocolService)
        {
            _protocolService = protocolService;
        }

        // Page Data
        public List<ProtocolDto> Protocols { get; set; } = new();
        public List<InvestigatorDto> Investigators { get; set; } = new();
        public List<string> AssignedInvestigatorIDs { get; set; } = new();
        public int TotalSites { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int CompletedCount { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public string TodayString { get; set; } =
            DateTime.UtcNow.ToString("yyyy-MM-dd");
        public string MaxDateString { get; set; } = "2035-12-31";

        // Create Protocol
        [BindProperty] public string Title { get; set; } = null!;
        [BindProperty] public string Phase { get; set; } = null!;
        [BindProperty] public DateTime StartDate { get; set; }
        [BindProperty] public DateTime EndDate { get; set; }
        [BindProperty] public string? Description { get; set; }

        // Edit Protocol
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

        // Edit Site
        [BindProperty] public Guid EditSiteID { get; set; }
        [BindProperty] public string EditSiteName { get; set; } = null!;
        [BindProperty] public string EditSiteLocation { get; set; } = null!;
        [BindProperty] public string EditSiteStatus { get; set; } = null!;
        [BindProperty] public Guid? EditSiteInvestigatorID { get; set; }
        [BindProperty] public Guid EditSiteProtocolID { get; set; }

        // GET
        public async Task OnGetAsync()
        {
            await _protocolService.AutoUpdateProtocolStatusesAsync();
            await LoadDataAsync();
        }

        // POST: Create Protocol
        public async Task<IActionResult> OnPostCreateProtocolAsync()
        {
            if (string.IsNullOrEmpty(Title))
            {
                ErrorMessage = "Title is required.";
                await LoadDataAsync();
                return Page();
            }

            if (EndDate.Date <= StartDate.Date)
            {
                ErrorMessage = "End date must be after the start date.";
                await LoadDataAsync();
                return Page();
            }

            var today = DateTime.UtcNow.Date;
            string autoStatus = today < StartDate.Date ? "Inactive" : "Active";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

            await _protocolService.CreateProtocolAsync(new CreateProtocolRequest
            {
                Title = Title,
                Phase = Phase,
                StartDate = StartDate,
                EndDate = EndDate,
                Status = autoStatus,
                Description = Description
            }, userId);

            SuccessMessage = $"Protocol '{Title}' created.";
            return RedirectToPage();
        }

        // POST: Edit Protocol
        public async Task<IActionResult> OnPostEditProtocolAsync()
        {
            if (string.IsNullOrEmpty(EditTitle))
            {
                ErrorMessage = "Title is required.";
                await LoadDataAsync();
                return Page();
            }

            if (EditEndDate.Date <= EditStartDate.Date)
            {
                ErrorMessage = "End date must be after the start date.";
                await LoadDataAsync();
                return Page();
            }

            var today = DateTime.UtcNow.Date;
            string newStatus;
            if (today < EditStartDate.Date) newStatus = "Inactive";
            else if (today <= EditEndDate.Date) newStatus = "Active";
            else newStatus = "Completed";

            var success = await _protocolService.UpdateProtocolAsync(
                EditProtocolID,
                new UpdateProtocolRequest
                {
                    Title = EditTitle,
                    Phase = EditPhase,
                    StartDate = EditStartDate,
                    EndDate = EditEndDate,
                    Status = newStatus,
                    Description = EditDescription
                });

            if (!success)
            {
                ErrorMessage = "Protocol not found.";
                await LoadDataAsync();
                return Page();
            }

            SuccessMessage = $"Protocol '{EditTitle}' updated.";
            return RedirectToPage();
        }

        // POST: Add Site
        public async Task<IActionResult> OnPostAddSiteAsync()
        {
            if (string.IsNullOrEmpty(SiteName) || string.IsNullOrEmpty(SiteLocation))
            {
                ErrorMessage = "Site name and location are required.";
                await LoadDataAsync();
                return Page();
            }

            var protocol = await _protocolService.GetProtocolByIdAsync(SiteProtocolID);
            if (protocol == null)
            {
                ErrorMessage = "Protocol not found.";
                await LoadDataAsync();
                return Page();
            }

            if (SiteInvestigatorID.HasValue)
            {
                var allProtocols = await _protocolService.GetAllProtocolsAsync();
                var alreadyAssigned = allProtocols
                    .SelectMany(p => p.Sites)
                    .Any(s => s.InvestigatorID == SiteInvestigatorID);

                if (alreadyAssigned)
                {
                    ErrorMessage = "This investigator is already assigned to another site.";
                    await LoadDataAsync();
                    return Page();
                }
            }

            await _protocolService.AddSiteAsync(SiteProtocolID, new CreateSiteRequest
            {
                Name = SiteName,
                Location = SiteLocation,
                Status = protocol.Status == "Completed" ? "Closed" : "Active",
                InvestigatorID = SiteInvestigatorID,
                ProtocolID = SiteProtocolID
            });

            SuccessMessage = $"Site '{SiteName}' added.";
            return RedirectToPage();
        }

        // POST: Edit Site
        public async Task<IActionResult> OnPostEditSiteAsync()
        {
            if (string.IsNullOrEmpty(EditSiteName) || string.IsNullOrEmpty(EditSiteLocation))
            {
                ErrorMessage = "Site name and location are required.";
                await LoadDataAsync();
                return Page();
            }

            if (EditSiteInvestigatorID.HasValue)
            {
                var allProtocols = await _protocolService.GetAllProtocolsAsync();
                var alreadyAssigned = allProtocols
                    .SelectMany(p => p.Sites)
                    .Any(s => s.InvestigatorID == EditSiteInvestigatorID
                           && s.SiteID != EditSiteID);

                if (alreadyAssigned)
                {
                    ErrorMessage = "This investigator is already assigned to another site.";
                    await LoadDataAsync();
                    return Page();
                }
            }

            var success = await _protocolService.UpdateSiteAsync(
                EditSiteID,
                new UpdateSiteRequest
                {
                    Name = EditSiteName,
                    Location = EditSiteLocation,
                    Status = EditSiteStatus,
                    InvestigatorID = EditSiteInvestigatorID
                });

            if (!success)
            {
                ErrorMessage = "Site not found.";
                await LoadDataAsync();
                return Page();
            }

            SuccessMessage = $"Site '{EditSiteName}' updated.";
            return RedirectToPage();
        }

        // POST: Delete Site
        public async Task<IActionResult> OnPostDeleteSiteAsync(Guid siteId)
        {
            var deleted = await _protocolService.DeleteSiteAsync(siteId);
            if (deleted) SuccessMessage = "Site deleted.";
            return RedirectToPage();
        }

        // POST: Delete Protocol
        public async Task<IActionResult> OnPostDeleteProtocolAsync(Guid protocolId)
        {
            await _protocolService.DeleteProtocolAsync(protocolId);
            SuccessMessage = "Protocol deleted.";
            return RedirectToPage();
        }

        // Load Data
        private async Task LoadDataAsync()
        {
            Protocols = await _protocolService.GetAllProtocolsAsync();
            Investigators = await _protocolService.GetInvestigatorsAsync();

            AssignedInvestigatorIDs = Protocols
                .SelectMany(p => p.Sites)
                .Where(s => s.InvestigatorID != null)
                .Select(s => s.InvestigatorID!.Value.ToString())
                .ToList();

            TotalSites = Protocols.Sum(p => p.Sites.Count);
            ActiveCount = Protocols.Count(p => p.Status == "Active");
            InactiveCount = Protocols.Count(p => p.Status == "Inactive");
            CompletedCount = Protocols.Count(p => p.Status == "Completed");
        }

        // Helpers
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
            string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpper();

        public string GetInvestigatorName(Guid? id) =>
            id == null ? "Not assigned"
            : Investigators.FirstOrDefault(u => u.ID == id.Value.ToString())?.FullName
              ?? "Unknown";
    }
}
