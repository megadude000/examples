using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class FCMomentsRepository : IFCMomentsRepository
    {
        private readonly DbContext dbContext;
        private readonly DbSet<FCMoment> fcMomentDataSet;

        public FCMomentsRepository(GazooContext dbContext)
        {
            this.dbContext = dbContext;
            fcMomentDataSet = dbContext.Set<FCMoment>();
        }

        public Task<string[]> GetImportFileNamesAsync(long importId)
        {
            return fcMomentDataSet
                .Where(moment => moment.ImportNumber == importId)
                .Select(moment => moment.Audio.CallId.ToString())
                .Distinct()
                .ToArrayAsync();
        }

        public Task AddRangeAsync(FCMoment[] itemRange)
        {
            fcMomentDataSet.AddRange(itemRange);
            return dbContext.SaveChangesAsync();
        }

        public Task<FCMoment> GetForProcessingAsync()
        {
            return fcMomentDataSet
               .Where(item => !item.SaveTime.HasValue && !item.InUse)
               .Include(moment => moment.Import)
               .OrderByDescending(item => item.Import.Priority ?? 0)
               .FirstOrDefaultAsync();
        }

        public Task<FCMoment> GetForVerificationAsync(long agentId)
        {
            return fcMomentDataSet
                 .Where(item => item.SaveTime.HasValue && !item.VerificationTime.HasValue)
                 .Where(item => !item.InUse && !item.VerifierId.HasValue)
                 .Where(item => item.AuthorId != agentId)
                 .Include(moment => moment.Import)
                 .OrderByDescending(item => item.Import.Priority ?? 0)
                 .FirstOrDefaultAsync();
        }

        public Task<FCMoment> GetAsync(long id)
        {
            return fcMomentDataSet
               .FirstOrDefaultAsync(item => item.Id == id);
        }

        public Task<FCMoment> GetByAudioIdAsync(long audioId)
        {
            return fcMomentDataSet
               .FirstOrDefaultAsync(item => item.AudioId == audioId);
        }

        public Task UpdateAsync(FCMoment item)
        {
            fcMomentDataSet.Update(item);
            return dbContext.SaveChangesAsync();
        }

        public Task<long[]> GetInUseIds()
        {
            return fcMomentDataSet
                .Where(moment => moment.InUse)
                .Select(moment => moment.Id)
                .ToArrayAsync();
        }
    }
}
