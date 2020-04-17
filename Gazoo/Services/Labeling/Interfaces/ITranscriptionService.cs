using Company.Gazoo.Requests.Labeling;
using Company.Gazoo.Responses.Labeling;
using System.IO;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling.Interfaces
{
    public interface ITranscriptionService
    {
        Task SaveAudioFileAsync(Stream stream, string boundary, long instanceId, long campaignId, long importNumber);
        Task<GetTranscriptionResponse> GetForTranscriptionAsync();
        Task<GetTranscriptionResponse> GetForVerificationAsync(long agentId);
        Task ReleaseTranscriptionAsync(ReleaseFromProcessingRequest request, long agentId);
        Task ReleaseTranscriptionAsync(long id);
        Task ReleaseAllTranscriptionsAsync();
        Task SaveAudioTrancriptionAsync(SaveTranscriptionRequest request, long agentId);
        Task SaveAudioVerificationAsync(SaveTranscriptionRequest transcription, long agentId);
        Task UpdateImportAsync(UpdateImportRequest request);
    }
}
