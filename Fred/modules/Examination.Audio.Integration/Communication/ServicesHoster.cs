using Company.Examination.Audio.Integration.Communication.Configuration;
using Company.Examination.Audio.Integration.Services.Interfaces;
using Company.Fred.Core.Configuration.Interfaces;
using Company.Fred.Core.Interfaces;
using Company.Fred.Subscription.Communication;
using Company.Fred.Subscription.Communication.Configuration.Interfaces;
using Company.Gazoo.Database.Enumerators;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Company.Gazoo.Communication.Contracts.AudioUpdateNotifications.AudioUpdateNotificationService;
using static Company.Gazoo.Communication.Contracts.SubscriptionActions.SubscriptionActionsService;

namespace Company.Examination.Audio.Integration.Communication
{
    public class ServicesHoster : SubscriptionActions, IServicesHoster
    {
        private readonly Server server;
        private readonly ILogger<ServicesHoster> logger;
        private readonly IRemoteConfigurationService remoteConfigurationService;
        private readonly IExaminationAudioProcessingService examinationAudioProcessingService;
        private readonly IExaminationDependentServicesConfiguration dependentServicesConfiguration;
        private readonly IExaminationInstanceConfiguration instanceConfiguration;

        public ServicesHoster(ILogger<ServicesHoster> logger,
            IRetryConfiguration retryConfiguration,
            AudioUpdateNotificationServiceBase audioUpdateService,
            IRemoteConfigurationService remoteConfigurationService,
            IExaminationInstanceConfiguration instanceConfiguration,
            Func<ChannelBase, SubscriptionActionsServiceClient> clientCreator,
            IExaminationHostedServicesConfiguration hostedServiceConfiguration,
            ISubscriptionActionsConfiguration subscriptionActionsConfiguration,
            IExaminationAudioProcessingService examinationAudioProcessingService,
            IExaminationDependentServicesConfiguration dependentServicesConfiguration)
            : base(logger,
                 instanceConfiguration,
                 retryConfiguration,
                 hostedServiceConfiguration.ExaminationActionsConfiguration,
                 subscriptionActionsConfiguration,
                 clientCreator)
        {
            this.logger = logger;
            this.instanceConfiguration = instanceConfiguration;
            this.remoteConfigurationService = remoteConfigurationService;
            this.dependentServicesConfiguration = dependentServicesConfiguration;
            this.examinationAudioProcessingService = examinationAudioProcessingService;

            server = new Server
            {
                Services =
                {
                     BindService(audioUpdateService)
                },
                Ports =
                {
                    new ServerPort(hostedServiceConfiguration.ExaminationActionsConfiguration.Ip, hostedServiceConfiguration.ExaminationActionsConfiguration.Port, ServerCredentials.Insecure)
                }
            };
        }

        public void Host()
        {
            if (!instanceConfiguration.IsSyncEnabled)
                return;

            logger.LogInformation("Fred instance uses module of synchronization examination audios from central Gazoo.");

            server.Start();
            Subscribe(SynchronizeChanges, ApplicationType.Examination);

            logger.LogInformation("Hosting services started");
        }

        public async Task Shutdown()
        {
            if (!instanceConfiguration.IsSyncEnabled)
                return;

            try
            {
                if (!string.IsNullOrEmpty(dependentServicesConfiguration.ExaminationIntegrationConfiguration.Ip))
                {
                    await Unsubscribe(ApplicationType.Examination);
                }

                await GrpcEnvironment.ShutdownChannelsAsync();
                await server.ShutdownAsync();

                logger.LogInformation("Hosting services stopped");
            }
            catch (ApplicationException ex)
            {
                logger.LogError(ex, $"Error during trying to define default IPv4 address.");
            }
            catch (RpcException rpcEx)
            {
                logger.LogError(rpcEx, "Error when unsubscribing examination notifications from central Gazoo.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when disposing server.");
            }
        }

        private async Task SynchronizeChanges()
        {
            try
            {
                logger.LogInformation("Start sending GetMissingFiles request to central Gazoo");
                await examinationAudioProcessingService.GetMissingFiles();
                await remoteConfigurationService.GetInstanceId();
                subscribeLoop.Dispose();
            }
            catch (RpcException rpcEx)
            {
                logger.LogError(rpcEx, "Error when sending GetMissingFiles request to central Gazoo");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error was occured while sending GetMissingFiles request to central Gazoo");
            }
        }
    }
}
