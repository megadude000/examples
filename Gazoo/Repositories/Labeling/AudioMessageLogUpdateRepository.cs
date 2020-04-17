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
    public class AudioMessageLogUpdateRepository : IAudioMessageLogUpdateRepository
    {
        private readonly GazooContext dbContext;
        private readonly DbSet<AudioMessageLogUpdate> audioMessageLogUpdatesDataSet;
        private readonly IMapper mapper;

        public AudioMessageLogUpdateRepository(GazooContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            audioMessageLogUpdatesDataSet = dbContext.Set<AudioMessageLogUpdate>();
            this.mapper = mapper;
        }

        public Task<AudioMessageLogUpdateResponse[]> GetResponseByCallIdAsync(long callId)
            => audioMessageLogUpdatesDataSet
                .Where(clientUtterance => clientUtterance.CallId == callId)
                .ProjectTo<AudioMessageLogUpdateResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        public Task<AudioMessageLogUpdate[]> GetByCallIdAsync(long callId)
            => audioMessageLogUpdatesDataSet
                .Where(audioMessageLogUpdate => audioMessageLogUpdate.CallId == callId)
                .ToArrayAsync();

        public Task<bool> ExistAsync(long withCallId)
            => audioMessageLogUpdatesDataSet
                .Where(audioMessageLogUpdate => audioMessageLogUpdate.CallId == withCallId)
                .AnyAsync();

        public Task AddRangeAsync(AudioMessageLogUpdate[] audioMessageLogUpdates)
        {
            audioMessageLogUpdatesDataSet.AddRange(audioMessageLogUpdates);
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateRangeAsync(AudioMessageLogUpdate[] audioMessageLogUpdates)
        {
            audioMessageLogUpdatesDataSet.UpdateRange(audioMessageLogUpdates);
            return dbContext.SaveChangesAsync();
        }

        public Task RemoveRangeAsync(AudioMessageLogUpdate[] audioMessageLogUpdates)
        {
            audioMessageLogUpdatesDataSet.RemoveRange(audioMessageLogUpdates);
            return dbContext.SaveChangesAsync();
        }
    }
}
