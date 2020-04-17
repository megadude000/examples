using Company.Examination.Audio.Integration.Communication.Configuration;
using Company.Examination.Audio.Integration.Communication.Services.Interfaces;
using Company.Fred.Core.Configuration.Interfaces;
using Company.Fred.Subscription.Communication.Services.Proxies;
using Company.Gazoo.Communication.Contracts.AudioUpdateNotifications;
using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Company.Gazoo.Communication.Contracts.AudioUpdateNotifications.AudioUpdateNotificationService;

namespace Company.Examination.Audio.Integration.Communication.Services
{
    internal class ExaminationIntegrationActionsProxy : CommunicationProxyBase<AudioUpdateNotificationServiceClient>, IExaminationIntegrationActionsProxy
    {
        public ExaminationIntegrationActionsProxy(IRetryConfiguration retryConfiguration,
            ILogger<ExaminationIntegrationActionsProxy> logger,
            IExaminationDependentServicesConfiguration dependentServices,
            Func<ChannelBase, AudioUpdateNotificationServiceClient> clientCreator)
            : base(dependentServices.ExaminationIntegrationConfiguration, retryConfiguration, logger, clientCreator)
        {
        }

        public async Task<ResponseStatus> GetMissingFiles(MissingFilesRequest request)
        {
            return await Request(async r => await r.GetMissingFilesAsync(request));
        }
    }
}
