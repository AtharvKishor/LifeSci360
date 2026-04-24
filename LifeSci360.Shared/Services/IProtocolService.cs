using LifeSci360.Shared.DTOs;

namespace LifeSci360.Shared.Services
{
    public interface IProtocolService
    {
        Task<List<ProtocolDto>> GetAllProtocolsAsync();
        Task<ProtocolDto?> GetProtocolByIdAsync(Guid id);
        Task<ProtocolDto> CreateProtocolAsync(CreateProtocolRequest request, string userId);
        Task<bool> UpdateProtocolAsync(Guid id, UpdateProtocolRequest request);
        Task<bool> DeleteProtocolAsync(Guid id);
        Task<bool> AddSiteAsync(Guid protocolId, CreateSiteRequest request);
        Task<bool> UpdateSiteAsync(Guid siteId, UpdateSiteRequest request);
        Task<List<ProtocolDto>> SearchProtocolsAsync(string? title, string? phase, string? status);
        Task<List<InvestigatorDto>> GetInvestigatorsAsync(); 
    }
}