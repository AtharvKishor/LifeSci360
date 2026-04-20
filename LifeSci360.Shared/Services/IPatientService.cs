using LifeSci360.Shared.DTOs;

namespace LifeSci360.Shared.Services
{
    public interface IPatientService
    {
        // Patients
        Task<List<PatientDto>> GetAllPatientsAsync();
        Task<PatientDto?> GetPatientByIdAsync(Guid id);
        Task<int> GetTotalPatientsAsync();
        Task<int> GetEnrolledCountAsync();
        Task UpdatePatientAsync(PatientDto patient);
        Task WithdrawPatientAsync(Guid patientId);
        Task UpdatePatientStatusAsync(Guid patientId, string status);

        // Enrollment
        Task EnrollPatientAsync(PatientDto patient);

        // Visits
        Task<List<VisitDto>> GetVisitsByPatientAsync(Guid patientId);
        Task UpdateVisitDateAsync(Guid visitId, DateTime newDate);
        Task CompleteVisitAsync(Guid visitId);
        Task AddVisitAsync(VisitDto visit);
        Task DeleteVisitAsync(Guid visitId);

        // Protocols
        Task<List<KeyValuePair<Guid, string>>> GetProtocolsAsync();
    }
}