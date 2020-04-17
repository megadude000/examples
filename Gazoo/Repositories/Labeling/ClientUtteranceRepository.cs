using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Company.Gazoo.Responses.Labeling;
using Microsoft.EntityFrameworkCore;

namespace Company.Gazoo.Repositories.Labeling
{
    public class ClientUtteranceRepository : IClientUtteranceRepository
    {
        private readonly GazooContext dbContext;
        private readonly DbSet<ClientUtterance> clientUtteranceDataSet;
        private readonly IMapper mapper;

        public ClientUtteranceRepository(GazooContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            clientUtteranceDataSet = dbContext.Set<ClientUtterance>();
            this.mapper = mapper;
        }

        public Task<ClientUtteranceResponse[]> GetResponseByCallIdAsync(long callId)
            => clientUtteranceDataSet
                .Where(clientUtterance => clientUtterance.CallId == callId)
                .ProjectTo<ClientUtteranceResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        public Task<ClientUtterance[]> GetByCallIdAsync(long callId)
            => clientUtteranceDataSet
                .Where(clientUtterance => clientUtterance.CallId == callId)
                .ToArrayAsync();

        public Task AddRangeAsync(ClientUtterance[] clientUtterances)
        {
            clientUtteranceDataSet.AddRange(clientUtterances);
            return dbContext.SaveChangesAsync();
        }

        public Task RemoveRangeAsync(ClientUtterance[] clientUtterances)
        {
            clientUtteranceDataSet.RemoveRange(clientUtterances);
            return dbContext.SaveChangesAsync();
        }
    }
}
