using LifeSci360.Identity.API.Data;
using LifeSci360.Identity.API.Models;
using LifeSci360.Shared.DTOs;
using LifeSci360.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace LifeSci360.Identity.API.Services
{
    public class PatientService : IPatientService
    {
        private readonly AppIdentityDbContext _context;

        public PatientService(AppIdentityDbContext context)
        {
            _context = context;
        }

        public async Task<List<PatientDto>> GetAllPatientsAsync()
        {
            return await _context.Patients
                .OrderByDescending(p => p.EnrolledDate)
                .Select(p => new PatientDto
                {
                    PatientID = p.PatientID,
                    ProtocolID = p.ProtocolID,
                    FullName = p.FullName,
                    DateOfBirth = p.DateOfBirth,
                    ContactInfo = p.ContactInfo,
                    Status = p.Status,
                    EnrolledDate = p.EnrolledDate
                })
                .ToListAsync();
        }

        public async Task<PatientDto?> GetPatientByIdAsync(Guid id)
        {
            var p = await _context.Patients.FindAsync(id);
            if (p == null) return null;

            return new PatientDto
            {
                PatientID = p.PatientID,
                ProtocolID = p.ProtocolID,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth,
                ContactInfo = p.ContactInfo,
                Status = p.Status,
                EnrolledDate = p.EnrolledDate
            };
        }

        public Task<int> GetTotalPatientsAsync()
            => _context.Patients.CountAsync();

        public Task<int> GetEnrolledCountAsync()
            => _context.Patients.CountAsync(p => p.Status == "Enrolled");

        public async Task UpdatePatientAsync(PatientDto dto)
        {
            var patient = await _context.Patients.FindAsync(dto.PatientID);
            if (patient == null) return;
            patient.FullName = dto.FullName;
            patient.ContactInfo = dto.ContactInfo;
            await _context.SaveChangesAsync();
        }

        public async Task WithdrawPatientAsync(Guid patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return;
            patient.Status = "Withdrawn";
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePatientStatusAsync(Guid patientId, string status)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return;
            patient.Status = status;
            await _context.SaveChangesAsync();
        }

        public async Task EnrollPatientAsync(PatientDto dto)
        {
            var patient = new Patient
            {
                PatientID = Guid.NewGuid(),
                ProtocolID = dto.ProtocolID,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                ContactInfo = dto.ContactInfo,
                Status = "Enrolled",
                EnrolledDate = DateTime.UtcNow
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            await GenerateVisitsAsync(patient);
        }

        private async Task GenerateVisitsAsync(Patient patient)
        {
            int[] offsets = { 0, 7, 28, 56, 84 };
            string[] names =
            {
                "Baseline",
                "Week 1",
                "Week 4",
                "Week 8",
                "End of Study"
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                _context.Visits.Add(new Visit
                {
                    VisitID = Guid.NewGuid(),
                    PatientID = patient.PatientID,
                    ProtocolID = patient.ProtocolID,
                    ScheduledDate = patient.EnrolledDate.AddDays(offsets[i]),
                    Status = "Scheduled",
                    Notes = names[i]
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<VisitDto>> GetVisitsByPatientAsync(Guid patientId)
        {
            return await _context.Visits
                .Where(v => v.PatientID == patientId)
                .OrderBy(v => v.ScheduledDate)
                .Select(v => new VisitDto
                {
                    VisitID = v.VisitID,
                    PatientID = v.PatientID,
                    ProtocolID = v.ProtocolID,
                    ScheduledDate = v.ScheduledDate,
                    Status = v.Status,
                    Notes = v.Notes
                })
                .ToListAsync();
        }

        public async Task UpdateVisitDateAsync(Guid visitId, DateTime newDate)
        {
            var visit = await _context.Visits.FindAsync(visitId);
            if (visit == null) return;

            if (visit.ScheduledDate.Date == newDate.Date) return;

            visit.ScheduledDate = newDate;
            visit.Status = "Rescheduled";
            await _context.SaveChangesAsync();
        }

        public async Task CompleteVisitAsync(Guid visitId)
        {
            var visit = await _context.Visits.FindAsync(visitId);
            if (visit == null) return;

            visit.Status = "Completed";
            await _context.SaveChangesAsync();
        }

        public async Task AddVisitAsync(VisitDto dto)
        {
            _context.Visits.Add(new Visit
            {
                VisitID = Guid.NewGuid(),
                PatientID = dto.PatientID,
                ProtocolID = dto.ProtocolID,
                ScheduledDate = dto.ScheduledDate,
                Status = "Scheduled",
                Notes = dto.Notes
            });
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVisitAsync(Guid visitId)
        {
            var visit = await _context.Visits.FindAsync(visitId);
            if (visit == null) return;
            _context.Visits.Remove(visit);
            await _context.SaveChangesAsync();
        }

        public async Task<List<KeyValuePair<Guid, string>>> GetProtocolsAsync()
        {
            return await _context.Protocols
                .Select(p => new KeyValuePair<Guid, string>(p.ProtocolID, p.Title))
                .ToListAsync();
        }
    }
}