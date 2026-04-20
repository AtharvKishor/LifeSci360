using LifeSci360.Shared.DTOs;
using LifeSci360.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LifeSci360.Identity.API.Pages.Dashboard
{
    [Authorize(Roles = "ClinicalTrialManager")]
    public class ClinicalManagerModel : PageModel
    {
        private readonly IPatientService _patientService;

        public ClinicalManagerModel(IPatientService patientService)
        {
            _patientService = patientService;
        }

        public List<PatientDto> Patients { get; set; } = new();
        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int CompletedPatients { get; set; }
        public int WithdrawnCount { get; set; }
        public HashSet<Guid> CompletedPatientIds { get; set; } = new();

        public async Task OnGetAsync()
        {
            Patients = await _patientService.GetAllPatientsAsync();
            TotalPatients = Patients.Count;
            var today = DateTime.UtcNow.Date;

            foreach (var patient in Patients)
            {
                var visits = await _patientService
                    .GetVisitsByPatientAsync(patient.PatientID);
                if (visits.Any() &&
                    visits.All(v => v.ScheduledDate.Date < today))
                {
                    CompletedPatientIds.Add(patient.PatientID);

                    if (patient.Status != "Completed" &&
                        patient.Status != "Withdrawn")
                        await _patientService.UpdatePatientStatusAsync(
                            patient.PatientID, "Completed");
                }
            }

            WithdrawnCount = Patients.Count(p => p.Status == "Withdrawn");
            CompletedPatients = CompletedPatientIds.Count;
            ActivePatients = TotalPatients - CompletedPatients - WithdrawnCount;
        }

        public async Task<IActionResult> OnPostWithdrawAsync(Guid patientId)
        {
            await _patientService.WithdrawPatientAsync(patientId);
            return RedirectToPage();
        }
    }
}