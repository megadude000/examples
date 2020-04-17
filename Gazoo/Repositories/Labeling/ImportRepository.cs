using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Database.Entities.Enums;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Company.Gazoo.Responses.Labeling;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class ImportRepository : IImportRepository
    {
        private readonly DbContext dbContext;
        private readonly DbSet<ImportNumber> importNumberDataSet;
        private readonly DbSet<ImportStatistics> importStatisticsDataSet;

        private readonly IMapper mapper;

        public ImportRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;
            importNumberDataSet = dbContext.Set<ImportNumber>();
            importStatisticsDataSet = dbContext.Set<ImportStatistics>();
        }

        public Task ResetRecordsVerificationAsync(long importId, LabelingType type)
        {
            return dbContext.Database.ExecuteSqlInterpolatedAsync($"select labeling.reset_records_verification({importId},{type})");
        }

        public Task<ImportStatisticsResponse[]> GetTranscriptionStatisticsAsync()
        {
            return importStatisticsDataSet.FromSqlRaw("select * from labeling.get_transcription_import_statistics()")
                .ProjectTo<ImportStatisticsResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();
        }

        public Task<ImportStatisticsResponse[]> GetFCMomentStatisticsAsync()
        {
            return importStatisticsDataSet.FromSqlRaw("select * from labeling.get_full_conversation_moments_import_statistics()")
                .ProjectTo<ImportStatisticsResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();
        }

        public Task AddAsync(ImportNumber newImport)
        {
            importNumberDataSet.Add(newImport);

            return dbContext.SaveChangesAsync();
        }

        public Task<ImportNumber> GetAsync(long id)
        {
            return importNumberDataSet
                .SingleOrDefaultAsync(import => import.Id == id);
        }

        public Task UpdateAsync(ImportNumber import)
        {
            importNumberDataSet.Update(import);

            return dbContext.SaveChangesAsync();
        }
    }
}
