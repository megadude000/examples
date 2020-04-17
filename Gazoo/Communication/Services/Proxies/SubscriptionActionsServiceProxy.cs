using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Company.Gazoo.Communication.Contracts.SubscriptionActions;
using Company.Gazoo.Communication.Services.Interfaces;
using Company.Gazoo.Infrastructure.Interfaces;
using Grpc.Core;
using System;
using System.Threading.Tasks;
using static Company.Gazoo.Communication.Contracts.SubscriptionActions.SubscriptionActionsService;

namespace Company.Gazoo.Communication.Services.Proxies
{
    public class SubscriptionActionsServiceProxy : SubscriptionActionsServiceBase, ISubscriptionActionsService
    {
        private readonly ILifetimeScopeExecutor lifetimeScopeExecutor;

        public SubscriptionActionsServiceProxy(ILifetimeScopeExecutor lifetimeScopeExecutor)
        {
            this.lifetimeScopeExecutor = lifetimeScopeExecutor;
        }

        public override Task<ResponseStatus> Subscribe(SubscriptionRequest request, ServerCallContext context)
        {
            return lifetimeScopeExecutor.ExecuteInNewScope<ISubscriptionActionsService, ResponseStatus>(service => service.Subscribe(request, context));

        }

        public override Task<ResponseStatus> Unsubscribe(UnsubscriptionRequest request, ServerCallContext context)
        {
            return lifetimeScopeExecutor.ExecuteInNewScope<ISubscriptionActionsService, ResponseStatus>(service => service.Unsubscribe(request, context));
        }
    }
}
