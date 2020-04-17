using Company.Gazoo.Database.Entities.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface ITranscriptionMetricsRepository
    {
        Task AddAsync(TranscriptionMetrics audio);
        Task<TranscriptionMetrics> GetByIdAsync(long id);
    }
}
