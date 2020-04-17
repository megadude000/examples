using Company.Gazoo.Models.AudioFile;
using Company.Gazoo.Requests.Labeling;
using Company.Gazoo.Responses.Labeling;
using System.IO;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling.Interfaces
{
    public interface IFCMomentsService
    {
        Task<SaveAudioResult> SaveAudioWithDataAsync(Stream stream, string boundary);
        Task<GetFCMomentResponse> GetForProcessingAsync();
        Task<GetFCMomentResponse> GetForVerificationAsync(long userId);
        Task ReleaseMomentAsync(ReleaseFromProcessingRequest request, long id);
        Task ReleaseMomentAsync(long id);
        Task ReleaseAllMomentsAsync();
        Task SaveResultAsync(SaveFCMomentResultRequest request, long autorId);
        Task SaveVerificationResultAsync(SaveFCMomentResultRequest request, long autorId);
    }
}
