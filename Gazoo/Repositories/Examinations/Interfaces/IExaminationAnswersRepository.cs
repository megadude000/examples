using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Database.Enumerators;
using Company.Gazoo.Models.Examinations.RequestModels;
using Company.Gazoo.Responses;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Examinations.Interfaces
{
    public interface IExaminationAnswersRepository
    {
        Task AddAsync(Answer answerValue);
        Task SoftDeleteAsync(long answerId);
        Task<AnswerResponse[]> GetAsync(AnswerType type, long setId);
        Task<Answer[]> GetAllAsync(long setId);
        Task<ExaminationAnswerModel[]> GetAsync(long setId);
        Task<ExaminationAnswerModel[]> GetDefaultAnswersAsync();
        Task<bool> ExistsAsync(string name, AnswerType type, long setId);
    }
}
