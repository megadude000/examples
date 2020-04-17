using Company.Pebbles.Examination.Models;
using System.Threading.Tasks;
using static Company.Pebbles.Configuration.Modules.ConfigurationModule;

namespace Company.Pebbles.Examination.Services.Interfaces
{
    public interface IHttpRequestService
    {
        Task<ExaminationModel[]> GetExaminationsAsync();
        Task<ExaminationAnswerModel[]> GetAnswersAsync(long examinationId);
        Task SaveExaminationReportAsync(ExaminationReportsInformationModel reportInformation);
        Task<ExaminationRemoteSettings> GetExaminationSettingsAsync();
        Task<ExaminationQuestionModel[]> GetExaminationDataAsync(long currentExaminationId);
    }
}
