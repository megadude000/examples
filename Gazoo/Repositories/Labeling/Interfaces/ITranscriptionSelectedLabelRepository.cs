using Company.Gazoo.Database.Entities.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface ITranscriptionSelectedLabelRepository
    {
        Task AddAsync(TranscriptionSelectedLabel selectedLabel);
        Task AddRangeAsync(TranscriptionSelectedLabel[] selectedLabels);
        Task<TranscriptionSelectedLabel[]> GetAssignedAsync(long transcriptionId);
        Task RemoveRangeAsync(TranscriptionSelectedLabel[] labels);
    }
}
