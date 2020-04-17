using Company.Pebbles.Common.Interfaces;
using Company.Pebbles.Configuration.Configuration;
using Company.Pebbles.Examination.Services.Interfaces;
using System.Threading.Tasks;

namespace Company.Pebbles.Examination.AppStartup
{
    internal class GetExaminationRemoteSettings: IExaminationLogonScopeProcess
    {
        public ProcessOrder Priority => ProcessOrder.Normal;

        public string Title => "Get examination settings from Company.Fred";

        private readonly IHttpRequestService examinationHttpRequestService;
        private IExaminationRemoteSettings examinationRemoteSettings;

        public GetExaminationRemoteSettings(IHttpRequestService examinationHttpRequestService,
            IExaminationRemoteSettings examinationRemoteSettings)
        {
            this.examinationHttpRequestService = examinationHttpRequestService;
            this.examinationRemoteSettings = examinationRemoteSettings;
        }

        public Task InitializeAsync()
        {
            return GetExaminationSettings();
        }

        private async Task GetExaminationSettings()
        {
            var responce = await examinationHttpRequestService.GetExaminationSettingsAsync();
            examinationRemoteSettings.CentralInstanceConnectionString = responce.CentralInstanceConnectionString;
            examinationRemoteSettings.FTPExaminationDirectory = responce.FTPExaminationDirectory;
            examinationRemoteSettings.InstanceId = responce.InstanceId;
        }
    }
}
