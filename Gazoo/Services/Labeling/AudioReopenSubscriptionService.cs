using Company.Common.Cache;
using Company.Gazoo.Configuration;
using Company.Gazoo.Configuration.KeepAlive;
using Company.Gazoo.Database.Entities.Enums;
using Company.Gazoo.Infrastructure.Interfaces;
using Company.Gazoo.Services.Labeling.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling
{
    internal class AudioReopenSubscriptionService : IAudioReopenSubscriptionService, IDisposable
    {
        private readonly ILogger<AudioReopenSubscriptionService> logger;
        private readonly ILifetimeScopeExecutor lifetimeScopeExecutor;
        private readonly SlidingExpirationCache<KeyValuePair<long, LabelingType>, long> subscribers;

        public AudioReopenSubscriptionService(ILabelingModuleConfiguration labelingModuleConfiguration,
            IKeepAliveConfiguration keepAliveConfiguration,
            ILogger<AudioReopenSubscriptionService> logger,
            ILifetimeScopeExecutor lifetimeScopeExecutor)
        {
            this.logger = logger;
            this.lifetimeScopeExecutor = lifetimeScopeExecutor;

            var cacheOptions = new SlidingExpirationCacheOptions<KeyValuePair<long, LabelingType>, long>()
            {
                ExpirationAsyncCallback = SubscriptionExpirationCallback,
                ExpirationScanFrequency = labelingModuleConfiguration.AudioReopen.ExpirationScanFrequency,
                RefreshFrequency = keepAliveConfiguration.Server.CacheRefreshFrequency,
                EntrySlidingExpiration = labelingModuleConfiguration.AudioReopen.AudioSlidingExpiration
            };
            subscribers = new SlidingExpirationCache<KeyValuePair<long, LabelingType>, long>(logger, cacheOptions);
        }

        public void Subscribe(long id, LabelingType type)
        {
            subscribers.AddOrUpdate(new KeyValuePair<long, LabelingType>(id, type), id);
            logger.LogInformation($"Created subscription for labeling {id}.");
        }

        public void Unsubscribe(long id, LabelingType type)
        {
            if (subscribers.TryGetValue(new KeyValuePair<long, LabelingType>(id, type), out var _))
            {
                subscribers.Remove(new KeyValuePair<long, LabelingType>(id, type));
                logger.LogInformation($"Subscription for labeling {id} has been removed.");
            }
        }

        private async Task SubscriptionExpirationCallback(KeyValuePair<long, LabelingType> key, long value)
        {
            logger.LogInformation($"Labeling: {value} subscription expired.");
            switch (key.Value)
            {
                case LabelingType.Transcription:
                    await lifetimeScopeExecutor.ExecuteInNewScope<ITranscriptionService>(service => service.ReleaseTranscriptionAsync(value));
                    break;

                case LabelingType.FullConversationMoments:
                    await lifetimeScopeExecutor.ExecuteInNewScope<IFCMomentsService>(service => service.ReleaseMomentAsync(value));
                    break;

                default:
                    break;
            }
        }

        public void Dispose()
        {
            subscribers?.Dispose();
        }
    }
}