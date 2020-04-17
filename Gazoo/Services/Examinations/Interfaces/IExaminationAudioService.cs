using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Requests;
using System.IO;
using System.Threading.Tasks;


namespace Company.Gazoo.Services.Examinations.Interfaces
{
    public interface IExaminationAudioService
    {
        Task<Stream> GetAsync(long id);
        Task<bool> UpdateAsync(UpdateExaminationAudioFileInfoRequest request);
        Task<SaveExaminationAudioResult> SaveAsync(Stream stream, string boundary);
        Task<DeleteExaminationAudioResult> DeleteAsync(long[] filesIds);
    }
}
