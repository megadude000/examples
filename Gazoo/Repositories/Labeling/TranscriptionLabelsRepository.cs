using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class TranscriptionLabelsRepository : ITranscriptionLabelsRepository
    {
        private readonly DbContext dbContext;
        private readonly DbSet<TranscriptionLabel> labelsDataSet;

        public TranscriptionLabelsRepository(GazooContext dbContext)
        {
            this.dbContext = dbContext;
            labelsDataSet = dbContext.Set<TranscriptionLabel>();
        }

        public Task AddAsync(TranscriptionLabel label)
        {
            labelsDataSet.Add(label);
            return dbContext.SaveChangesAsync();
        }

        public Task AddRangeAsync(TranscriptionLabel[] labels)
        {
            labelsDataSet.AddRange(labels);
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(TranscriptionLabel label)
        {
            labelsDataSet.Update(label);
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateRangeAsync(TranscriptionLabel[] labels)
        {
            labelsDataSet.UpdateRange(labels);
            return dbContext.SaveChangesAsync();
        }

        public Task<TranscriptionLabel> GetByIdAsync(long id)
            => labelsDataSet
                .Where(label => label.Id == id)
                .Where(label => !label.IsDeleted)
                .SingleOrDefaultAsync();

        public Task<TranscriptionLabel[]> GetAllByGroupIdAsync(long groupId)
        {
            return labelsDataSet
                .Where(label => label.LabelGroupId == groupId)
                .Where(label => !label.IsDeleted)
                .ToArrayAsync();
        }

        public Task<bool> CheckIfExistsAsync(string name, long groupId)
        {
            return labelsDataSet
                .Where(label => label.LabelGroupId == groupId)
                .Where(label => !label.IsDeleted)
                .AnyAsync(label => label.Name.ToUpper() == name.ToUpper());
        }
    }
}
