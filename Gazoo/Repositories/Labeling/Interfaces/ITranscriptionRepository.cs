using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Models.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface ITranscriptionRepository
    {
        Task AddAsync(Transcription audioFile);
        Task<Transcription> GetAsync(long id);
        Task<Transcription> GetByAudioIdAsync(long audioId);
        Task<Transcription> GetForTranscriptionAsync();
        Task<Transcription> GetForVerificationAsync(long agentId);
        Task<Transcription> GetForCurrentDayVerificationAsync(long agentId);
        Task UpdateAsync(Transcription audioFile);
        Task<long[]> GetInUseIds();
        Task<string> GetAgentTranscriptionAsync(long id);
        Task<string[]> GetImportFileNamesAsync(long importId);
        Task<CampaignInstancePairing[]> GetCampaignInstancePairingAsync();
    }
}
