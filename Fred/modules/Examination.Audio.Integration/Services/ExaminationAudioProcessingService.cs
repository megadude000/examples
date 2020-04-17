using Company.Common.AspNet.Infrastructure.NetworkAddress;
using Company.Examination.Audio.Integration.Communication.Configuration;
using Company.Examination.Audio.Integration.Communication.Services.Interfaces;
using Company.Examination.Audio.Integration.Configuration.Interfaces;
using Company.Examination.Audio.Integration.Services.Interfaces;
using Company.Fred.Core.Interfaces;
using Company.Fred.Subscription.Communication.Configuration.Interfaces;
using Company.Gazoo.Communication.Contracts.AudioUpdateNotifications;
using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Examination.Audio.Integration.Services
{
    internal class ExaminationAudioProcessingService : IExaminationAudioProcessingService
    {
        private readonly ILogger<ExaminationAudioProcessingService> logger;
        private readonly IExaminationHostedServicesConfiguration hostedServices;
        private readonly IExaminationIntegrationActionsProxy examinationIntegrationActionsProxy;
        private readonly IExaminationAudioSynchronizationConfiguration examinationAudioSynchronizationConfiguration;

        public ExaminationAudioProcessingService(
            ILogger<ExaminationAudioProcessingService> logger,
            IExaminationHostedServicesConfiguration hostedServices,
            IExaminationIntegrationActionsProxy examinationIntegrationActionsProxy,
            IExaminationAudioSynchronizationConfiguration examinationAudioSynchronizationConfiguration)
        {
            this.logger = logger;
            this.hostedServices = hostedServices;
            this.examinationIntegrationActionsProxy = examinationIntegrationActionsProxy;
            this.examinationAudioSynchronizationConfiguration = examinationAudioSynchronizationConfiguration;
        }

        public async Task GetMissingFiles()
        {
            var request = GetMissingFilesRequest();
            var result = await examinationIntegrationActionsProxy.GetMissingFiles(request);

            if (result.NotificationStatus == NotificationStatus.Successful)
                logger.LogInformation("GetMissingFiles request finished successfully");
            else
                logger.LogInformation("GetMissingFiles request failed");
        }

        private MissingFilesRequest GetMissingFilesRequest()
        {
            var files = Directory.EnumerateFiles(examinationAudioSynchronizationConfiguration.AudioFilesWorkingDirectory);
            var resultFileName = files.Select(audio => Path.GetFileNameWithoutExtension(audio));
            var repeatedFiles = new RepeatedField<string>();
            repeatedFiles.AddRange(resultFileName);

            return new MissingFilesRequest
            {
                FileNames = { repeatedFiles },
                Endpoint = $"{NetworkAddressUtils.GetDefaultIPv4Address()}:{hostedServices.ExaminationActionsConfiguration.Port}"
            };
        }
    }
}
