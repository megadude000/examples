using System.Threading.Tasks;
using Company.Gazoo.Communication.Contracts.AudioUpdateNotifications;
using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Grpc.Core;

namespace Company.Gazoo.Communication.Services.Interfaces
{
    public interface IExaminationIntegrationActionsService
    {
        Task<ResponseStatus> GetMissingFiles(MissingFilesRequest request, ServerCallContext context);
    }
}
