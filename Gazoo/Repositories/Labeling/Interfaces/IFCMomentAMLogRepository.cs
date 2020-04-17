using Company.Gazoo.Database.Entities.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface IFCMomentAMLogRepository
    {
        Task AddAsync(FCMomentAMLog log);
        Task<FCMomentAMLog[]> GetRangeByMomentIdAsync(long momentId);
    }
}
