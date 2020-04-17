using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Responses;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Examinations.Interfaces
{
    public interface IExaminationQuestionAudioFilesRepository
    {
        Task AddAsync(QuestionAudioFile audioFile);
        Task<QuestionAudioFile> GetAsync(long id);
        Task<QuestionAudioFile[]> GetRangeByNamesAsync(string[] fileNames);
        Task UpdateAsync(QuestionAudioFile audioFile);
        Task SoftDeleteRangeAsync(long[] filesIds);
        Task<string[]> GetNamesRangeAsync(long[] filesIds);
        Task<AudioFileResponse[]> GetAllAsync();
        Task<string[]> GetNamesAsync();
        Task<bool> ExistsAsync(string fileName);
    }
}
