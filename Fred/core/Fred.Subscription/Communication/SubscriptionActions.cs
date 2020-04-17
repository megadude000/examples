using Company.Common.AspNet.Infrastructure.NetworkAddress;
using Company.Fred.Core.Configuration.Interfaces;
using Company.Fred.Subscription.Communication.Configuration.Interfaces;
using Company.Fred.Subscription.Communication.Services.Proxies;
using Company.Gazoo.Communication.Contracts.SubscriptionActions;
using Company.Gazoo.Database.Enumerators;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static Company.Gazoo.Communication.Contracts.SubscriptionActions.SubscriptionActionsService;

namespace Company.Fred.Subscription.Communication
{
    public abstract class SubscriptionActions : CommunicationProxyBase<SubscriptionActionsServiceClient>
    {
        private readonly ILogger<SubscriptionActions> logger;
        private readonly IInstanceConfiguration instanceConfiguration;
        private readonly IRetryConfiguration retryConfiguration;
        private readonly ICommunicationConfigurationSettings hostedService;

        protected IDisposable subscribeLoop;
        private bool isSubscribedToCentralGazoo = false;

        public SubscriptionActions(ILogger<SubscriptionActions> logger,
            IInstanceConfiguration instanceConfiguration,
            IRetryConfiguration retryConfiguration,
            ICommunicationConfigurationSettings hostedService,
            ICommunicationConfigurationSettings dependentService,
            Func<ChannelBase, SubscriptionActionsServiceClient> clientCreator)
            : base(dependentService, retryConfiguration, logger, clientCreator)
        {
            this.logger = logger;
            this.hostedService = hostedService;
            this.instanceConfiguration = instanceConfiguration;
            this.retryConfiguration = retryConfiguration;
        }

        public void Subscribe(Func<Task> SynchronizeChanges, ApplicationType applicationType)
        {
            subscribeLoop = Observable
               .Interval(retryConfiguration.Frequency)
               .TakeWhile(_ => !isSubscribedToCentralGazoo)
               .Select(start =>
                   Observable.FromAsync(() => SubscribeToCentralGazoo(applicationType))
                   .LastAsync())
               .Concat()
               .Subscribe(_ => { },
                   ex => logger.LogError(ex, $"Exception occured on subscribing loop to central Gazoo"),
                   async () => await SynchronizeChanges()
               );
        }

        public async Task Unsubscribe(ApplicationType applicationType)
        {
            logger.LogInformation("Sending request to Central Gazoo for unsubscribing.");

            var request = new UnsubscriptionRequest
            {
                Endpoint = instanceConfiguration.Name,
                Target = (int)applicationType
            };

            await Request(async x => await x.UnsubscribeAsync(request));
        }

        private async Task SubscribeToCentralGazoo(ApplicationType applicationType)
        {
            if (!isSubscribedToCentralGazoo)
            {
                try
                {
                    var request = new SubscriptionRequest
                    {
                        Endpoint = $"{NetworkAddressUtils.GetDefaultIPv4Address()}:{hostedService.Port}",
                        InstanceName = instanceConfiguration.Name,
                        Target = (int)applicationType
                    };

                    logger.LogInformation("Sending request to Central Gazoo for subscribing.");
                    await Request(async x => await x.SubscribeAsync(request));
                    isSubscribedToCentralGazoo = true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error was occured while trying subscribe local Gazoo.");
                }
            }
        }
    }
}
