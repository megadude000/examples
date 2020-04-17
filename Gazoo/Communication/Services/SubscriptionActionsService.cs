using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Company.Gazoo.Communication.Contracts.SubscriptionActions;
using Company.Gazoo.Communication.Services.Interfaces;
using Company.Gazoo.Database.Enumerators;
using Company.Gazoo.Repositories.ApplicationSubscriptions.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Company.Gazoo.Communication.Services
{
    internal class SubscriptionActionsService: ISubscriptionActionsService
    {
        private readonly ILogger<SubscriptionActionsService> logger;
        private readonly IApplicationSubscriptionRepository instanceSubscriptionRepository;

        public SubscriptionActionsService(ILogger<SubscriptionActionsService> logger,
                                          IApplicationSubscriptionRepository instanceSubscriptionRepository)
        {
            this.logger = logger;
            this.instanceSubscriptionRepository = instanceSubscriptionRepository;
        }

        public async Task<ResponseStatus> Subscribe(SubscriptionRequest request, ServerCallContext context)
        {
            var instance = await instanceSubscriptionRepository.GetInfoByNameAsync(request.InstanceName);
            if (instance == null)
            {
                await instanceSubscriptionRepository.AddAddressAsync(request.Endpoint, request.InstanceName, (ApplicationType)request.Target);
            }
            else if(instance.Address == request.Endpoint && instance.InstanceName != request.InstanceName)
            {
                return new ResponseStatus();
            }
            else if(instance.Address != request.Endpoint && instance.Target != (ApplicationType)request.Target)
            {
                await instanceSubscriptionRepository.UpdateAddressAndStatusAsync(request);
            }
            else
            {
                await instanceSubscriptionRepository.UpdateStatusAsync(request.Endpoint, InstanceStatus.Alive);
            }

            return new ResponseStatus();
        }

        public async Task<ResponseStatus> Unsubscribe(UnsubscriptionRequest request, ServerCallContext context)
        {
            await instanceSubscriptionRepository.UpdateStatusAsync(request.Endpoint, InstanceStatus.Dead);

            logger.LogInformation($"Removed subscriber from soundboard event bus: {request.Endpoint}.");
            return new ResponseStatus();
        }
    }
}
