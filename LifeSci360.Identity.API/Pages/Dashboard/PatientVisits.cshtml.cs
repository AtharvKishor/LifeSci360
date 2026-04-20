using LifeSci360.Shared.DTOs;
using LifeSci360.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LifeSci360.Identity.API.Pages.Dashboard
{
    [Authorize(Roles = "ClinicalTrialManager")]
    public class PatientVisitsModel : PageModel
    {
        private readonly IPatientService _patientService;

        public PatientVisitsModel(IPatientService patientService)
        {
            _patientService = patientService;
        }

        public PatientDto? Patient { get; set; }
        public List<VisitDto> UpcomingVisits { get; set; } = new();
        public List<VisitDto> CompletedVisits { get; set; } = new();

        [BindProperty]
        public Guid EditVisitId { get; set; }

        [BindProperty]
        public DateTime NewVisitDate { get; set; }

        [BindProperty]
        public string NewVisitName { get; set; } = string.Empty;

        [BindProperty]
        public DateTime NewVisitScheduledDate { get; set; }

        public async Task OnGetAsync(Guid patientId)
        {
            Patient = await _patientService.GetPatientByIdAsync(patientId);
            var today = DateTime.UtcNow.Date;
            var visits = await _patientService
                .GetVisitsByPatientAsync(patientId);

            UpcomingVisits = visits
                .Where(v => v.ScheduledDate.Date >= today)
                .OrderBy(v => v.ScheduledDate)
                .ToList();

            CompletedVisits = visits
                .Where(v => v.ScheduledDate.Date < today)
                .OrderByDescending(v => v.ScheduledDate)
                .ToList();
        }

        public async Task<IActionResult> OnPostRescheduleVisitAsync(Guid patientId)
        {
            await _patientService.UpdateVisitDateAsync(EditVisitId, NewVisitDate);
            return RedirectToPage(new { patientId });
        }

        public async Task<IActionResult> OnPostCompleteVisitAsync(Guid patientId)
        {
            await _patientService.CompleteVisitAsync(EditVisitId);
            return RedirectToPage(new { patientId });
        }

        public async Task<IActionResult> OnPostAddVisitAsync(Guid patientId)
        {
            if (string.IsNullOrEmpty(NewVisitName) ||
                NewVisitScheduledDate == default)
                return RedirectToPage(new { patientId });

            Patient = await _patientService.GetPatientByIdAsync(patientId);

            await _patientService.AddVisitAsync(new VisitDto
            {
                VisitID = Guid.NewGuid(),
                PatientID = patientId,
                ProtocolID = Patient!.ProtocolID,
                ScheduledDate = NewVisitScheduledDate,
                Status = "Scheduled",
                Notes = NewVisitName
            });

            return RedirectToPage(new { patientId });
        }

        public async Task<IActionResult> OnPostDeleteVisitAsync(Guid patientId)
        {
            await _patientService.DeleteVisitAsync(EditVisitId);
            return RedirectToPage(new { patientId });
        }
    }
}