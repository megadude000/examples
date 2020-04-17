using Company.Gazoo.Database.Entities.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface ILabelingAudioRepository
    {
        Task AddAsync(LabelingAudio audio);
        Task<LabelingAudio> GetByIdAsync(long id);
        Task<bool> IsExistAsync(string fileName, long instanceId, long campaignId);
    }
}
