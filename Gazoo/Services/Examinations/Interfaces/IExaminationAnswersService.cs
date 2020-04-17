using Company.Gazoo.Requests;
using Company.Gazoo.Responses;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations.Interfaces
{
    public interface IExaminationAnswersService
    {
        Task AddAsync(AddAnswerRequest request);
        Task UpdateSetAsync(UpdateAnswerSetRequest request);
        Task<AllAnswersResponse> GetAsync(long setId);
    }
}
