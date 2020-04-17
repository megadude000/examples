using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class TranscriptionMetricsRepository : ITranscriptionMetricsRepository
    {
        private readonly DbContext dbContext;
        private readonly DbSet<TranscriptionMetrics> transcriptionMetricsDataSet;

        public TranscriptionMetricsRepository(GazooContext dbContext)
        {
            this.dbContext = dbContext;
            transcriptionMetricsDataSet = dbContext.Set<TranscriptionMetrics>();
        }

        public Task AddAsync(TranscriptionMetrics audio)
        {
            transcriptionMetricsDataSet.Add(audio);
            return dbContext.SaveChangesAsync();
        }

        public Task<TranscriptionMetrics> GetByIdAsync(long id)
        {
            return transcriptionMetricsDataSet
                .FirstOrDefaultAsync(metric => metric.Id == id);
        }
    }
}
