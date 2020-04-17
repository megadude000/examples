using System.Threading.Tasks;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Responses;

namespace Company.Gazoo.Repositories.Examinations.Interfaces
{
    public interface IExaminationsRepository
    {
        Task AddAsync(Examination examination);
        Task UpdateAsync(Examination examination);
        Task SoftDeleteAsync(long examId);
        Task<Examination> GetAsync(long examId);
        Task<ExaminationResponse[]> GetAsync(ExaminationFilterModel filterModel);
        Task<ExaminationModel[]> GetModelAsync();
        Task<ExaminationInfoResponse> GetExaminationInfoResponseAsync(long examId);
        Task<bool> NameExistsAsync(string examTitle);
        Task<bool> AnswerSetIsUsedAsync(long answerSetId);
        Task<long> GetAnswerSetIdAsync(long examId);
        Task<ExaminationForReportResponse[]> GetForReportsAsync();
    }
}
