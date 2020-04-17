using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class LabelingAudioRepository : ILabelingAudioRepository
    {
        private readonly DbContext dbContext;
        private readonly DbSet<LabelingAudio> labelingAudioDataSet;

        public LabelingAudioRepository(GazooContext dbContext)
        {
            this.dbContext = dbContext;
            labelingAudioDataSet = dbContext.Set<LabelingAudio>();
        }

        public Task AddAsync(LabelingAudio audio)
        {
            labelingAudioDataSet.Add(audio);
            return dbContext.SaveChangesAsync();
        }

        public Task<LabelingAudio> GetByIdAsync(long id)
        {
            return labelingAudioDataSet
                .FirstOrDefaultAsync(audio => audio.Id == id);
        }

        public Task<bool> IsExistAsync(string fileName, long instanceId, long campaignId)
        {
            return labelingAudioDataSet
                .AnyAsync(audio => EF.Functions.ILike(audio.FileName, fileName) && audio.InstanceId == instanceId && audio.CampaignId == campaignId);
        }
    }
}
