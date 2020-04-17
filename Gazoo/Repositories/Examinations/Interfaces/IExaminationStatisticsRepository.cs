using Company.Gazoo.Models.Examinations;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Examinations.Interfaces
{
    public interface IExaminationStatisticsRepository
    {
        Task<ExaminationStatisticModel[]> GetAsync(ExaminationStatisticsFilter filter);
    }
}
