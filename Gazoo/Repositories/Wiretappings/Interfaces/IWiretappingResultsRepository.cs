using Company.BammBamm.Database.Entities.Campaigns;
using Company.Gazoo.Responses.Wiretapping;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Wiretappings.Interfaces
{
    public interface IWiretappingResultsRepository
    {
        Task AddRangeAsync(WiretappingResult[] results);
        Task UpdateRangeAsync(WiretappingResult[] results);
        Task<WiretappingResultResponse[]> GetAsync(long callId);
        Task<WiretappingResult[]> GetAsync(long callId, long[] factorIds);
        Task<bool> IsCallWiretappedAsync(long callId);
    }
}
