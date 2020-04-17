using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Responses.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface IClientUtteranceRepository
    {
        Task<ClientUtteranceResponse[]> GetResponseByCallIdAsync(long callId);
        Task<ClientUtterance[]> GetByCallIdAsync(long callId);
        Task AddRangeAsync(ClientUtterance[] clientUtterances);
        Task RemoveRangeAsync(ClientUtterance[] clientUtterances);
    }
}