using Company.Gazoo.Models.Wiretapping;
using Company.Gazoo.Requests.Wiretapping;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Wiretappings.Interfaces
{
    public interface IWiretappingFactorsService
    {
        Task AddFactorAsync(AddFactorRequest request);
        Task UpdateFactorAsync(UpdateFactorRequest request);
        Task ToggleFactorAsync(long factorId);
    }
}
