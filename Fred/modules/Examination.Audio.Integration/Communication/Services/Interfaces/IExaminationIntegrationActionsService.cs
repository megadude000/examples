using Company.Gazoo.Communication.Contracts.AudioUpdateNotifications;
using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Grpc.Core;
using System.Threading.Tasks;

namespace Company.Examination.Audio.Integration.Communication.Services.Interfaces
{
    public interface IExaminationIntegrationActionsService
    {
        Task<ResponseStatus> AudioUpdatesAvailable(NotificationRequest request, ServerCallContext context);
    }
}
