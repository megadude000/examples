using Autofac;
using static Company.Gazoo.Communication.Contracts.SubscriptionActions.SubscriptionActionsService;

namespace Company.Fred.Subscription.Modules
{
    public class CommunicationModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<SubscriptionActionsServiceClient>()
                .AsSelf();
        }
    }
}
