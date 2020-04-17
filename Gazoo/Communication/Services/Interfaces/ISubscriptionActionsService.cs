using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Company.Gazoo.Communication.Contracts.SubscriptionActions;
using Grpc.Core;
using System.Threading.Tasks;

namespace Company.Gazoo.Communication.Services.Interfaces
{
    public interface ISubscriptionActionsService
    {
        Task<ResponseStatus> Subscribe(SubscriptionRequest request, ServerCallContext context);

        Task<ResponseStatus> Unsubscribe(UnsubscriptionRequest request, ServerCallContext context);
    }
}
