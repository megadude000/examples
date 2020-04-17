using Company.Gazoo.Database.Entities.Enums;
using Company.Gazoo.Models.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling.Interfaces
{
    public interface IWorkingPeriodsService
    {
        Task SaveWorkingPeriodAsync(WorkingPeriodFilter filter, long spentTime);
    }
}
