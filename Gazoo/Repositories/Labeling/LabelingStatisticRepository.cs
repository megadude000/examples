using AutoMapper;
using AutoMapper.QueryableExtensions;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class LabelingStatisticRepository : ILabelingStatisticRepository
    {
        private readonly IMapper mapper;
        private readonly DbSet<GeneralStatistic> generalStatisticDataSet;
        private readonly DbSet<TranscriptionStatistic> transcriptionStatisticDataSet;
        private readonly DbSet<FCMomentStatistic> fullConversationMomentStatisticsDataSet;

        public LabelingStatisticRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            generalStatisticDataSet = dbContext.Set<GeneralStatistic>();
            transcriptionStatisticDataSet = dbContext.Set<TranscriptionStatistic>();
            fullConversationMomentStatisticsDataSet = dbContext.Set<FCMomentStatistic>();
        }

        public Task<GeneralStatisticModel[]> GetGeneralStatisticsAsync(long[] agentIds, DateTime fromDate, DateTime toDate)
        {
            return generalStatisticDataSet.FromSqlInterpolated($"select * from labeling.get_general_statistics({agentIds}, {fromDate}, {toDate})")
                .ProjectTo<GeneralStatisticModel>(mapper.ConfigurationProvider)
                .ToArrayAsync();
        }
       
        public Task<TranscriptionStatisticModel[]> GetTranscriptionStatisticsAsync(long[] agentIds, DateTime fromDate, DateTime toDate)
        {
            return transcriptionStatisticDataSet.FromSqlInterpolated($"select * from labeling.get_transcription_statistics({agentIds}, {fromDate}, {toDate})")
                 .ProjectTo<TranscriptionStatisticModel>(mapper.ConfigurationProvider)
                 .ToArrayAsync();
        }

        public Task<FCMomentStatisticModel[]> GetFCMomentStatisticsAsync(long[] agentIds, DateTime fromDate, DateTime toDate)
        {
            return fullConversationMomentStatisticsDataSet.FromSqlInterpolated($"select * from labeling.get_full_conversation_moments_statistics({agentIds}, {fromDate}, {toDate})")
                .ProjectTo<FCMomentStatisticModel>(mapper.ConfigurationProvider)
                .ToArrayAsync();
        }
    }
}
