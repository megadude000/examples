using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class TranscriptionSelectedLabelRepository : ITranscriptionSelectedLabelRepository
    {
        private readonly DbContext dbContext;
        private readonly DbSet<TranscriptionSelectedLabel> selectedLabelsDataSet;

        public TranscriptionSelectedLabelRepository(GazooContext dbContext)
        {
            this.dbContext = dbContext;
            selectedLabelsDataSet = dbContext.Set<TranscriptionSelectedLabel>();
        }

        public Task AddAsync(TranscriptionSelectedLabel selectedLabel)
        {
            selectedLabelsDataSet.Add(selectedLabel);
            return dbContext.SaveChangesAsync();
        }

        public Task AddRangeAsync(TranscriptionSelectedLabel[] selectedLabels)
        {
            selectedLabelsDataSet.AddRange(selectedLabels);
            return dbContext.SaveChangesAsync();
        }

        public Task<TranscriptionSelectedLabel[]> GetAssignedAsync(long transcriptionId)
        {
            return selectedLabelsDataSet
                .Where(item => item.TranscriptionId == transcriptionId)
                .ToArrayAsync();
        }

        public Task RemoveRangeAsync(TranscriptionSelectedLabel[] labels)
        {
            selectedLabelsDataSet.RemoveRange(labels);
            return dbContext.SaveChangesAsync();
        }
    }
}
