using Company.Gazoo.Requests;
using Company.Gazoo.Responses;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations.Interfaces
{
    public interface IExaminationsService
    {
        Task UpdateModificationTimeAsync(long examinationId);
        Task DeleteAsync(long examId);
        Task AddAsync(AddExaminationRequest request, long userId);
        Task UpdateAsync(UpdateExaminationRequest request);
        Task<ExaminationReportResponse> GetReportAsync(long reportId);
        Task<InstanceResponse[]> GetAvailableInstancesToUserAsync(ClaimsPrincipal user);
        Task ChangeRandomStateAsync(long examinationId);
    }
}
