using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Models.Examinations;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Examinations.Interfaces
{
    public interface IExaminationQuestionsRepository
    {
        Task AddAsync(Question question);
        Task UpdateAsync(Question question);
        Task UpdateRangeAsync(Question[] questions);
        Task SoftDeleteAsync(long questionId);
        Task<Question> GetAsync(long questionId);
        Task SoftDeleteRangeAsync(long examId);
        Task AddQuestionAnswersRangeAsync(QuestionAnswer[] questionAnswers);
        Task<QuestionResponse[]> GetResponseRangeAsync(long examId);
        Task<ExaminationQuestionModel[]> GetModelRangeAsync(long examId);
        Task<bool> AnswerIsUsedAsync(long answerId);
        Task<bool> AudioIsUsedAsync(long[] filesIds);
    }
}
