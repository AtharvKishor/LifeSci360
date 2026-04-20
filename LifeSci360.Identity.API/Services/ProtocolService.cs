using LifeSci360.Identity.API.Data;
using LifeSci360.Identity.API.Models;
using LifeSci360.Shared.DTOs;
using LifeSci360.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace LifeSci360.Identity.API.Services
{
    public class ProtocolService : IProtocolService
    {
        private readonly AppIdentityDbContext _context;

        public ProtocolService(AppIdentityDbContext context)
        {
            _context = context;
        }

        // ── GET ALL ─────────────────────────────────────────────
        public async Task<List<ProtocolDto>> GetAllProtocolsAsync()
        {
            var protocols = await _context.Protocols
                .Include(p => p.Sites)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return protocols.Select(p => MapToDto(p)).ToList();
        }

        // ── GET BY ID ───────────────────────────────────────────
        public async Task<ProtocolDto?> GetProtocolByIdAsync(int id)
        {
            var protocol = await _context.Protocols
                .Include(p => p.Sites)
                .FirstOrDefaultAsync(p => p.ProtocolID == id);

            return protocol == null ? null : MapToDto(protocol);
        }

        // ── CREATE ──────────────────────────────────────────────
        public async Task<ProtocolDto> CreateProtocolAsync(
            CreateProtocolRequest request, string userId)
        {
            var protocol = new Protocol
            {
                Title = request.Title,
                Phase = request.Phase,
                Status = request.Status,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Description = request.Description,
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

            // Reload with sites navigation
            await _context.Entry(protocol)
                .Collection(p => p.Sites)
                .LoadAsync();

            return MapToDto(protocol);
        }

        // ── UPDATE ──────────────────────────────────────────────
        public async Task<bool> UpdateProtocolAsync(
            int id, UpdateProtocolRequest request)
        {
            var protocol = await _context.Protocols.FindAsync(id);
            if (protocol == null) return false;

            protocol.Title = request.Title;
            protocol.Phase = request.Phase;
            protocol.Status = request.Status;
            protocol.StartDate = request.StartDate;
            protocol.EndDate = request.EndDate;
            protocol.Description = request.Description;

            _context.AuditLogs.Add(new AuditLog
            {
                UserID = "system",
                Action = $"Updated Protocol ID: {id} — {request.Title}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        // ── DELETE ───────
        public async Task<bool> DeleteProtocolAsync(int id)
        {
            var protocol = await _context.Protocols
                .Include(p => p.Sites)
                .FirstOrDefaultAsync(p => p.ProtocolID == id);

            if (protocol == null) return false;

            // Sites are cascade deleted automatically
            // due to OnDelete(DeleteBehavior.Cascade) in DbContext
            _context.Protocols.Remove(protocol);

            _context.AuditLogs.Add(new AuditLog
            {
                UserID = "system",
                Action = $"Deleted Protocol ID: {id} — {protocol.Title}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        // ── ADD SITE ────────────────────────────────────────────
        public async Task<bool> AddSiteAsync(
            int protocolId, CreateSiteRequest request)
        {
            var protocol = await _context.Protocols.FindAsync(protocolId);
            if (protocol == null) return false;

            var site = new Site
            {
                Name = request.Name,
                Location = request.Location,
                Status = request.Status,
                InvestigatorID = string.IsNullOrEmpty(request.InvestigatorID)
                                 ? null : request.InvestigatorID,
                ProtocolID = protocolId
            };

            _context.Sites.Add(site);

            _context.AuditLogs.Add(new AuditLog
            {
                UserID = "system",
                Action = $"Added Site '{site.Name}' " +
                            $"to Protocol ID: {protocolId}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        // ── UPDATE SITE ─────────────────────────────────────────
        public async Task<bool> UpdateSiteAsync(
            int siteId, UpdateSiteRequest request)
        {
            var site = await _context.Sites.FindAsync(siteId);
            if (site == null) return false;

            site.Name = request.Name;
            site.Location = request.Location;
            site.Status = request.Status;
            site.InvestigatorID = string.IsNullOrEmpty(request.InvestigatorID)
                                  ? null : request.InvestigatorID;

            _context.AuditLogs.Add(new AuditLog
            {
                UserID = "system",
                Action = $"Updated Site ID: {siteId} — {request.Name}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        // ── SEARCH ──────────────────────────────────────────────
        public async Task<List<ProtocolDto>> SearchProtocolsAsync(
            string? title, string? phase, string? status)
        {
            var query = _context.Protocols
                .Include(p => p.Sites)
                .AsQueryable();

            if (!string.IsNullOrEmpty(title))
                query = query.Where(p =>
                    p.Title.Contains(title));

            if (!string.IsNullOrEmpty(phase))
                query = query.Where(p => p.Phase == phase);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            var results = await query
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return results.Select(p => MapToDto(p)).ToList();
        }

        // ── GET INVESTIGATORS ───────────────────────────────────
        public async Task<List<InvestigatorDto>> GetInvestigatorsAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new InvestigatorDto
                {
                    ID = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!
                })
                .ToListAsync();
        }

        // ── PRIVATE MAPPER ──────────────────────────────────────
        private static ProtocolDto MapToDto(Protocol p)
        {
            return new ProtocolDto
            {
                ProtocolID = p.ProtocolID,
                Title = p.Title,
                Phase = p.Phase,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Description = p.Description,
                CreatedDate = p.CreatedDate,
                CreatedByUserID = p.CreatedByUserID,
                Sites = p.Sites.Select(s => new SiteDto
                {
                    SiteID = s.SiteID,
                    Name = s.Name,
                    Location = s.Location,
                    InvestigatorID = s.InvestigatorID,
                    Status = s.Status,
                    ProtocolID = s.ProtocolID
                }).ToList()
            };
        }
    }
}