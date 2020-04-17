using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Responses.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface IAudioMessageLogUpdateRepository
    {
        Task<AudioMessageLogUpdateResponse[]> GetResponseByCallIdAsync(long callId);
        Task<AudioMessageLogUpdate[]> GetByCallIdAsync(long callId);
        Task<bool> ExistAsync(long withCallId);
        Task AddRangeAsync(AudioMessageLogUpdate[] audioMessageLogUpdates);
        Task UpdateRangeAsync(AudioMessageLogUpdate[] audioMessageLogUpdates);
        Task RemoveRangeAsync(AudioMessageLogUpdate[] audioMessageLogUpdates);
    }
}
