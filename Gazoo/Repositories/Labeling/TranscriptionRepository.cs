using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class TranscriptionRepository : ITranscriptionRepository
    {
        private readonly DbContext dbContext;
        private readonly DbSet<Transcription> transcriptionDataSet;

        public TranscriptionRepository(GazooContext dbContext)
        {
            this.dbContext = dbContext;
            transcriptionDataSet = dbContext.Set<Transcription>();
        }

        public Task<CampaignInstancePairing[]> GetCampaignInstancePairingAsync()
        {
            return transcriptionDataSet
               .Where(transcription => transcription.ImportNumber.HasValue)
               .GroupBy(transcription => new { transcription.Audio.InstanceId, transcription.Audio.CampaignId })
               .Select(group => new CampaignInstancePairing
               {
                   InstanceId = group.Key.InstanceId,
                   CampaignId = group.Key.CampaignId
               })
               .ToArrayAsync();
        }

        public Task<string[]> GetImportFileNamesAsync(long importId)
        {
            return transcriptionDataSet
                .Where(transcription => transcription.ImportNumber == importId)
                .Select(transcription => transcription.Audio.FileName)
                .ToArrayAsync();
        }

        public Task<Transcription> GetForTranscriptionAsync()
        {
            return transcriptionDataSet
               .Where(transcription => !transcription.SaveTime.HasValue && !transcription.InUse)
               .Include(transcription => transcription.Metrics)
               .Include(transcription => transcription.Import)
               .OrderByDescending(item => item.Import.Priority ?? 0)
               .ThenBy(item => item.Metrics.DeepSpeechTranscriptionScore ?? double.MinValue)
               .FirstOrDefaultAsync();
        }

        public Task<string> GetAgentTranscriptionAsync(long id)
        {
            return transcriptionDataSet
               .Where(transcription => transcription.Id == id)
               .Select(transcription => transcription.AgentTranscription)
               .SingleOrDefaultAsync();
        }

        public Task<long[]> GetInUseIds()
        {
            return transcriptionDataSet
                .Where(transcription => transcription.InUse)
                .Select(transcription => transcription.Id)
                .ToArrayAsync();
        }

        public Task<Transcription> GetForVerificationAsync(long agentId)
        {
            return transcriptionDataSet
               .Where(transcription => transcription.SaveTime.HasValue && !transcription.VerificationTime.HasValue)
               .Where(transcription => !transcription.InUse && !transcription.VerifierId.HasValue)
               .Where(transcription => transcription.AgentId != agentId)
               .Include(transcription => transcription.Metrics)
               .Include(transcription => transcription.Import)
               .OrderByDescending(item => item.Import.Priority ?? 0)
               .ThenByDescending(item => item.Metrics.WordErrorRate ?? double.MaxValue)
               .ThenByDescending(item => item.Metrics.CharErrorRate ?? double.MaxValue)
               .FirstOrDefaultAsync();
        }

        public Task<Transcription> GetForCurrentDayVerificationAsync(long agentId)
        {
            return transcriptionDataSet
              .Where(transcription => transcription.SaveTime.HasValue && transcription.SaveTime.Value.Date == DateTime.UtcNow.Date)
              .Where(transcription => !transcription.VerificationTime.HasValue)
              .Where(transcription => !transcription.InUse && !transcription.VerifierId.HasValue)
              .Where(transcription => transcription.AgentId != agentId)
              .Include(transcription => transcription.Metrics)
              .Include(transcription => transcription.Import)
              .OrderByDescending(item => item.Import.Priority ?? 0)
              .ThenByDescending(item => item.Metrics.WordErrorRate ?? double.MaxValue)
              .ThenByDescending(item => item.Metrics.CharErrorRate ?? double.MaxValue)
              .FirstOrDefaultAsync();
        }

        public Task UpdateAsync(Transcription transcription)
        {
            transcriptionDataSet.Update(transcription);
            return dbContext.SaveChangesAsync();
        }

        public Task<Transcription> GetAsync(long id)
        {
            return transcriptionDataSet
                .Include(transcription => transcription.Metrics)
                .FirstOrDefaultAsync(transcription => transcription.Id == id);
        }

        public Task<Transcription> GetByAudioIdAsync(long audioId)
        {
            return transcriptionDataSet
                .FirstOrDefaultAsync(transcription => transcription.AudioId == audioId);
        }

        public Task AddAsync(Transcription transcription)
        {
            transcriptionDataSet.Add(transcription);
            return dbContext.SaveChangesAsync();
        }
    }
}
