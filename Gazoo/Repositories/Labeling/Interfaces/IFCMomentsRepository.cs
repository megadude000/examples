using Company.Gazoo.Database.Entities.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface IFCMomentsRepository
    {
        Task AddRangeAsync(FCMoment[] itemRange);
        Task UpdateAsync(FCMoment item);
        Task<FCMoment> GetAsync(long id);
        Task<FCMoment> GetByAudioIdAsync(long audioId);
        Task<FCMoment> GetForProcessingAsync();
        Task<FCMoment> GetForVerificationAsync(long agentId);
        Task<long[]> GetInUseIds();
        Task<string[]> GetImportFileNamesAsync(long importId);
    }
}
