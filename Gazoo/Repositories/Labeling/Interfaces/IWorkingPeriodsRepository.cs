using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Database.Entities.Enums;
using System.Threading.Tasks;
using Company.Gazoo.Models.Labeling;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface IWorkingPeriodsRepository
    {
        Task<WorkingPeriod> GetAsync(WorkingPeriodFilter filter);
        Task AddAsync(WorkingPeriod time);
        Task UpdateAsync(WorkingPeriod time);
    }
}
