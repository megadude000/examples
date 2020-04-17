using System.Threading.Tasks;

namespace Company.Examination.Audio.Integration.Services.Interfaces
{
    public interface IExaminationAudioProcessingService
    {
        Task GetMissingFiles();
    }
}
