using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Database.Entities.Enums;
using System.Threading.Tasks;
using Company.Gazoo.Responses.Labeling;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface IImportRepository
    {
        Task<ImportStatisticsResponse[]> GetTranscriptionStatisticsAsync();
        Task<ImportStatisticsResponse[]> GetFCMomentStatisticsAsync();
        Task AddAsync(ImportNumber newImport);
        Task<ImportNumber> GetAsync(long id);
        Task UpdateAsync(ImportNumber import);
        Task ResetRecordsVerificationAsync(long importId, LabelingType type);
    }
}
