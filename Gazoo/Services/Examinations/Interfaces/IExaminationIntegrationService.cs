using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Models.Examinations.RequestModels;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations.Interfaces
{
    public interface IExaminationIntegrationService
    {
        Task SaveReportAsync(ExaminationReportsInformationModel report);
        Task<ExaminationAnswerModel[]> GetAnswersAsync(long examinationId);
    }
}
