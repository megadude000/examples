using Company.Gazoo.Requests.Wiretapping;
using Company.Gazoo.Responses.Wiretapping;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Wiretappings.Interfaces
{
    public interface IWiretappingResultsService
    {
        Task<WiretapperDetailsResponse> AddResultAsync(AddWiretappingResultRequest request, long userId);
        Task<WiretapperDetailsResponse> UpdateResultAsync(UpdateWiretappingResultRequest request, long userId);
    }
}
