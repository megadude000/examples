using AutoMapper;
using AutoMapper.QueryableExtensions;
using Company.BammBamm.Database.Contexts;
using Company.BammBamm.Database.Entities.Campaigns;
using Company.Gazoo.Repositories.Wiretappings.Interfaces;
using Company.Gazoo.Responses.Wiretapping;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Wiretappings
{
    internal class WiretappingResultsRepository : IWiretappingResultsRepository
    {
        private readonly IMapper mapper;
        private readonly BammBammContext bammBammContext;
        private readonly DbSet<WiretappingResult> wiretappingResultsDataSet;

        public WiretappingResultsRepository(BammBammContext bammBammContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.bammBammContext = bammBammContext;

            wiretappingResultsDataSet = bammBammContext.Set<WiretappingResult>();
        }

        public Task AddRangeAsync(WiretappingResult[] results)
        {
            wiretappingResultsDataSet.AddRange(results);

            return bammBammContext.SaveChangesAsync();
        }

        public Task UpdateRangeAsync(WiretappingResult[] results)
        {
            wiretappingResultsDataSet.UpdateRange(results);

            return bammBammContext.SaveChangesAsync();
        }

        public Task<WiretappingResultResponse[]> GetAsync(long callId)
            => wiretappingResultsDataSet
                .Where(result => result.CallResultId == callId)
                .OrderByDescending(result => result.FactorId)
                .ProjectTo<WiretappingResultResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        public Task<WiretappingResult[]> GetAsync(long callId, long[] factorIds)
            => wiretappingResultsDataSet
                .Where(result => result.CallResultId == callId)
                .Where(result => factorIds.Contains(result.FactorId))
                .ToArrayAsync();

        public Task<bool> IsCallWiretappedAsync(long callResultId)
            => wiretappingResultsDataSet
                .AnyAsync(result => result.CallResultId == callResultId);
    }
}
