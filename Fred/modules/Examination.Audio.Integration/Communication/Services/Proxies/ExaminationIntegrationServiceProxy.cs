using Company.Examination.Audio.Integration.Communication.Services.Interfaces;
using Company.Fred.Core.Infrastructure.Interfaces;
using Company.Gazoo.Communication.Contracts.AudioUpdateNotifications;
using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Grpc.Core;
using System.Threading.Tasks;
using static Company.Gazoo.Communication.Contracts.AudioUpdateNotifications.AudioUpdateNotificationService;

namespace Company.Examination.Audio.Integration.Communication.Services.Proxies
{
    internal class ExaminationIntegrationServiceProxy : AudioUpdateNotificationServiceBase, IExaminationIntegrationActionsService
    {
        private readonly ILifetimeScopeExecutor lifetimeScopeExecutor;

        public ExaminationIntegrationServiceProxy(ILifetimeScopeExecutor lifetimeScopeExecutor)
        {
            this.lifetimeScopeExecutor = lifetimeScopeExecutor;
        }

        public override Task<ResponseStatus> AudioUpdatesAvailable(NotificationRequest request, ServerCallContext context)
        {
            return lifetimeScopeExecutor.ExecuteInNewScope<IExaminationIntegrationActionsService, ResponseStatus>(service => service.AudioUpdatesAvailable(request, context));
        }
    }
}
