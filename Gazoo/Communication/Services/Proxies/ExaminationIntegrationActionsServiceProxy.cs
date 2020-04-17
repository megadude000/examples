using System.Threading.Tasks;
using Company.Gazoo.Communication.Contracts.AudioUpdateNotifications;
using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Company.Gazoo.Communication.Services.Interfaces;
using Company.Gazoo.Infrastructure.Interfaces;
using Grpc.Core;
using static Company.Gazoo.Communication.Contracts.AudioUpdateNotifications.AudioUpdateNotificationService;

namespace Company.Gazoo.Communication.Services.Proxies
{
    internal class ExaminationIntegrationActionsServiceProxy : AudioUpdateNotificationServiceBase, IExaminationIntegrationActionsService
    {
        private readonly ILifetimeScopeExecutor lifetimeScopeExecutor;

        public ExaminationIntegrationActionsServiceProxy(ILifetimeScopeExecutor lifetimeScopeExecutor)
        {
            this.lifetimeScopeExecutor = lifetimeScopeExecutor;
        }

        public override Task<ResponseStatus> GetMissingFiles(MissingFilesRequest request, ServerCallContext context)
        {
            return lifetimeScopeExecutor.ExecuteInNewScope<IExaminationIntegrationActionsService, ResponseStatus>(service => service.GetMissingFiles(request, context));
        }
    }
}