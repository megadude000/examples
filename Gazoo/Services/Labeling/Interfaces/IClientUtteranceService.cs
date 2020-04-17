using Company.Gazoo.Requests.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling.Interfaces
{
    public interface IClientUtteranceService
    {
        Task UpdateRecordingLabelingAsync(LabelingChankCreatorRequest request, long agentId);
    }
}