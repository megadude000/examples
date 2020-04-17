using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Requests.Labeling;
using System.IO;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling.Interfaces
{
    public interface ILabelingAudioService
    {
        Task<long> SaveAudioFileAsync(Stream fileStream, FCMomentAudioFileInfo fileInfo);
        Task<long> SaveAudioFileAsync(MemoryStream stream, string boundary, long instanceId, long campaignId);
        Task<Stream> GetAudioFileAsync(long id);
        Task<long> GetNewImportNumber(GetImportNumberRequest request);
    }
}
