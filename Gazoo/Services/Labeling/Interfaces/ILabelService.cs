using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Requests.Labeling;
using Company.Gazoo.Responses.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling.Interfaces
{
    public interface ILabelService
    {
        Task SaveSelectedLabelsAsync(long[] selectedLabels, long? transcriptionId, long? recordingAudioChunkId);
        Task AddLabelGroupAsync(AddLabelGroupRequest request);
        Task UpdateLabelGroupAsync(UpdateLabelGroupRequest labelGroup);
        Task SoftDeleteLabelGroupAsync(long labelGroupId);
        Task SoftDeleteLabelAsync(long labelId);
        Task DeleteLabelsByGroupIdAsync(long labelGroupId);
        Task<TranscriptionLabelModel[]> GetTranscriptionLabelsAsync(long groupId);
    }
}
