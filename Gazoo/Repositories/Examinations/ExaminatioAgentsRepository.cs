using Company.Gazoo.DbContexts;
using Microsoft.EntityFrameworkCore;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using System.Threading.Tasks;
using Company.Gazoo.Responses;
using AutoMapper;
using Company.Gazoo.Requests;
using System.Linq;
using AutoMapper.QueryableExtensions;

namespace Company.Gazoo.Repositories.Examinations
{
    internal class ExaminationAgentsRepository : IExaminationAgentsRepository
    {
        private readonly IMapper mapper;
        private readonly GazooContext dbContext;
        private readonly DbSet<Agent> agentsDataSet;

        public ExaminationAgentsRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;

            agentsDataSet = dbContext.Set<Agent>();
        }

        public Task AddAsync(Agent agent)
        {
            agentsDataSet.Add(agent);
            return dbContext.SaveChangesAsync();
        }

        public Task<long> GetPebblesAgentIdAsync(long instanceId, long agentId)
            => agentsDataSet
                .Where(agent => agent.LocalAgentId.HasValue)
                .Where(agent => agent.InstanceId == instanceId && agent.LocalAgentId.Value == agentId)
                .Select(agent => agent.Id)
                .FirstOrDefaultAsync();

        public Task<AgentForReportResponse[]> GetBarneyAgentsAsync(SearchRequest searchRequest)
            => agentsDataSet
                .Where(agent => !agent.LocalAgentId.HasValue)
                .Where(agent => string.IsNullOrWhiteSpace(searchRequest.SearchString) || EF.Functions.ILike(agent.Surname + " " + agent.Name, '%' + searchRequest.SearchString + '%'))
                .OrderByDescending(agent => agent.Id)
                .Take(searchRequest.ResultsLimit)
                .ProjectTo<AgentForReportResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        public Task<AgentForReportResponse[]> GetPebblesAgentsAsync(SearchRequest searchRequest)
            => agentsDataSet
               .Where(agent => agent.LocalAgentId.HasValue && agent.InstanceId.HasValue)
               .Where(agent => searchRequest.AvailableInstances.Any(availableInstance => availableInstance == agent.InstanceId.Value))
               .Where(agent => string.IsNullOrWhiteSpace(searchRequest.SearchString) || EF.Functions.ILike(agent.Surname + " " + agent.Name, '%' + searchRequest.SearchString + '%'))
               .OrderByDescending(agent => agent.Id)
               .Take(searchRequest.ResultsLimit)
               .Include(agent => agent.Instance)
               .ProjectTo<AgentForReportResponse>(mapper.ConfigurationProvider)
               .ToArrayAsync();
    }
}
