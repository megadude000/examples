using Company.Gazoo.Requests;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations.Interfaces
{
    public interface IExaminationQuestionsService
    {
        Task AddAsync(AddQuestionRequest request);
        Task DeleteAsync(long questionId);
        Task UpdateAsync(UpdateQuestionRequest request);
    }
}
