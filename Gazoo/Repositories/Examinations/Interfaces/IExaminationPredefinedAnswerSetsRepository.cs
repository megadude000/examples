using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Responses;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Examinations.Interfaces
{
    public interface IExaminationPredefinedAnswerSetsRepository
    {
        Task AddAsync(PredefinedAnswerSet answerSet);
        Task UpdateAsync(PredefinedAnswerSet answerSet);
        Task DeleteAsync(long answerSetId);
        Task<PredefinedAnswerSetResponse[]> GetAsync();
        Task<PredefinedAnswerSet> GetAsync(long id);
        Task<bool> ExistsAsync(string name);
        Task<string> GetNameAsync(long answerSetId);
    }
}
