using Company.Gazoo.Database.Entities.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface ITranscriptionLabelsRepository
    {
        Task AddAsync(TranscriptionLabel label);
        Task AddRangeAsync(TranscriptionLabel[] labels);
        Task UpdateAsync(TranscriptionLabel label);
        Task UpdateRangeAsync(TranscriptionLabel[] labels);
        Task<TranscriptionLabel> GetByIdAsync(long id);
        Task<TranscriptionLabel[]> GetAllByGroupIdAsync(long groupId);
        Task<bool> CheckIfExistsAsync(string name, long groupId);
    }
}
