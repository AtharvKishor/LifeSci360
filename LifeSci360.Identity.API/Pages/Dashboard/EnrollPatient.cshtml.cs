using LifeSci360.Shared.DTOs;
using LifeSci360.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LifeSci360.Identity.API.Pages.Dashboard
{
    [Authorize(Roles = "ClinicalTrialManager")]
    public class EnrollPatientModel : PageModel
    {
        private readonly IPatientService _patientService;

        public EnrollPatientModel(IPatientService patientService)
        {
            _patientService = patientService;
        }

        public List<KeyValuePair<Guid, string>> Protocols { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public PatientDto NewPatient { get; set; } = new();

        public async Task OnGetAsync()
        {
            Protocols = await _patientService.GetProtocolsAsync();
        }

        public async Task<IActionResult> OnPostEnrollAsync()
        {
            if (string.IsNullOrEmpty(NewPatient.FullName) ||
                NewPatient.ProtocolID == Guid.Empty ||
                NewPatient.DateOfBirth == default)
            {
                ErrorMessage = "Please fill all required fields.";
                Protocols = await _patientService.GetProtocolsAsync();
                return Page();
            }

            NewPatient.Status = "Screened";
            await _patientService.EnrollPatientAsync(NewPatient);
            return RedirectToPage("/Dashboard/ClinicalManager");
        }
    }
}