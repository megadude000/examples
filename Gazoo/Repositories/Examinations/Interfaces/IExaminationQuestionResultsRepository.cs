using System.Threading.Tasks;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Responses;

namespace Company.Gazoo.Repositories.Examinations.Interfaces
{
    public interface IExaminationQuestionResultsRepository
    {
        Task AddResultAsync(QuestionResult questionResult);
        Task AddResultAnswerAsync(QuestionResultAnswers questionResultAnswer);
        Task AddResultAnswerRangeAsync(QuestionResultAnswers[] questionResultAnswers);
        Task<QuestionResultResponse[]> GetResultsAsync(long reportId);
    }
}
