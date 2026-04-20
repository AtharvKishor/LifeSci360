using LifeSci360.Shared.DTOs;

namespace LifeSci360.Shared.Services
{
    public interface IProtocolService
    {
        Task<List<ProtocolDto>> GetAllProtocolsAsync();
        Task<ProtocolDto?> GetProtocolByIdAsync(int id);
        Task<ProtocolDto> CreateProtocolAsync(CreateProtocolRequest request, string userId);
        Task<bool> UpdateProtocolAsync(int id, UpdateProtocolRequest request);
        Task<bool> DeleteProtocolAsync(int id);
        Task<bool> AddSiteAsync(int protocolId, CreateSiteRequest request);
        Task<bool> UpdateSiteAsync(int siteId, UpdateSiteRequest request);
        Task<List<ProtocolDto>> SearchProtocolsAsync(string? title, string? phase, string? status);
        Task<List<InvestigatorDto>> GetInvestigatorsAsync(); 
    }
}