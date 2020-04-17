using Company.Pebbles.Configuration.Configuration;
using Company.Pebbles.Core.Services.Interfaces;
using Company.Pebbles.Examination.Helpers;
using Company.Pebbles.Examination.Models;
using Company.Pebbles.Examination.Services.Interfaces;
using System.Threading.Tasks;
using static Company.Pebbles.Configuration.Modules.ConfigurationModule;

namespace Company.Pebbles.Examination.Services
{
    internal class HttpRequestService : IHttpRequestService
    {
        private readonly ISettings settings;
        private readonly IHttpSenderService httpSenderService;
        private readonly IExaminationRemoteSettings examinationRemoteSettings;

        public HttpRequestService(
            ISettings settings,
            IHttpSenderService httpSenderService,
            IExaminationRemoteSettings examinationRemoteSettings)
        {
            this.settings = settings;
            this.httpSenderService = httpSenderService;
            this.examinationRemoteSettings = examinationRemoteSettings;
        }

        public async Task<ExaminationModel[]> GetExaminationsAsync()
        {
            var url = $"{examinationRemoteSettings.CentralInstanceConnectionString}{ExaminationRequestsConstants.GetExaminations}";
            var response = await httpSenderService.Get(url);
            return httpSenderService.ParseResponse<ExaminationModel[]>(response);
        }

        public async Task<ExaminationAnswerModel[]> GetAnswersAsync(long examinationId)
        {
            var url = $"{examinationRemoteSettings.CentralInstanceConnectionString}{ExaminationRequestsConstants.GetExaminationAnswers}/{examinationId}";
            var response = await httpSenderService.Get(url);
            return httpSenderService.ParseResponse<ExaminationAnswerModel[]>(response);
        }

        public async Task SaveExaminationReportAsync(ExaminationReportsInformationModel reportInformation)
        {
            var url = $"{examinationRemoteSettings.CentralInstanceConnectionString}{ExaminationRequestsConstants.SaveExaminationReports}";
            await httpSenderService.Post(reportInformation, url);
        }

        public async Task<ExaminationRemoteSettings> GetExaminationSettingsAsync()
        {
            var url = $"{settings.Integration.ConnectionString}{ExaminationRequestsConstants.GetExaminationRemoteSettings}";
            var response = await httpSenderService.Get(url);
            return httpSenderService.ParseResponse<ExaminationRemoteSettings>(response);
        }

        public async Task<ExaminationQuestionModel[]> GetExaminationDataAsync(long examinationId)
        {
            var url = $"{examinationRemoteSettings.CentralInstanceConnectionString}{ExaminationRequestsConstants.GetExaminationData}/{examinationId}";
            var response = await httpSenderService.Get(url);
            return httpSenderService.ParseResponse<ExaminationQuestionModel[]>(response);
        }
    }
}
