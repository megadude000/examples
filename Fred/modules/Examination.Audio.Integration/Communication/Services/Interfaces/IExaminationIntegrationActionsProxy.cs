using Company.Gazoo.Communication.Contracts.AudioUpdateNotifications;
using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using System.Threading.Tasks;

namespace Company.Examination.Audio.Integration.Communication.Services.Interfaces
{
    public interface IExaminationIntegrationActionsProxy
    {
        Task<ResponseStatus> GetMissingFiles(MissingFilesRequest request);
    }
}
