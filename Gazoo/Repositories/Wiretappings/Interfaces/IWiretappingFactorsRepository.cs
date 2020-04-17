using Company.BammBamm.Database.Entities.Campaigns;
using Company.Gazoo.Models.Wiretapping;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Wiretappings.Interfaces
{
    public interface IWiretappingFactorsRepository
    {
        Task AddAsync(WiretappingFactor newImport);
        Task<WiretappingFactor> GetAsync(long id);
        Task UpdateAsync(WiretappingFactor import);
        Task<bool> ExistsAsync(string factorName);
        Task<WiretappingFactorModel[]> GetFactorsAsync();
        Task<WiretappingFactorModel[]> GetEnabledFactorsAsync();
    }
}
