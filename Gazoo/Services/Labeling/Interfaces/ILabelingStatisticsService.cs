using Company.Gazoo.Models.User;
using Company.Gazoo.Requests;
using Company.Gazoo.Requests.Labeling;
using Company.Gazoo.Responses.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling.Interfaces
{
    public interface ILabelingStatisticsService
    {
        Task<UserModel[]> GetAgentsForReportsAsync(SearchRequest searchRequest);
        Task<GeneralLabelingStatisticResponse> GetGeneralStatisticsAsync(LabelingStatisticRequest request);
        Task<TranscriptionStatisticsResponse> GetTranscriptionStatisticsAsync(LabelingStatisticRequest request);
        Task<FCMomentStatisticsResponse> GetFCMomentStatisticsAsync(LabelingStatisticRequest request);
        Task<ImportStatisticsResponse[]> GetImportStatisticsAsync(GetImportsReportRequest reqest);
    }
}
