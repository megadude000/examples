using System.Threading.Tasks;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Models.Examinations;

namespace Company.Gazoo.Repositories.Examinations.Interfaces
{
    public interface IExaminationResultsRepository
    {
        Task AddAsync(ExaminationResult result);
        Task<ExaminationReportInfo[]> GetBarneyResultsAsync(ExaminationReportFilter filter);
        Task<ExaminationReportInfo[]> GetPebblesResultsAsync(ExaminationReportFilter filter);
        Task<ExaminationResult> GetAsync(long id);
    }
}
