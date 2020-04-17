using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class FCMomentAMLogRepository : IFCMomentAMLogRepository
    {
        private readonly DbContext dbContext;
        private readonly DbSet<FCMomentAMLog> fcMomentAMLogDataSet;

        public FCMomentAMLogRepository(GazooContext dbContext)
        {
            this.dbContext = dbContext;
            fcMomentAMLogDataSet = dbContext.Set<FCMomentAMLog>();
        }

        public Task AddAsync(FCMomentAMLog log)
        {
            fcMomentAMLogDataSet.Add(log);
            return dbContext.SaveChangesAsync();
        }

        public Task<FCMomentAMLog[]> GetRangeByMomentIdAsync(long momentId)
        {
            return fcMomentAMLogDataSet
                .Where(item => item.MomentId == momentId)
                .OrderBy(item => item.Id)
                .ToArrayAsync();
        }
    }
}
