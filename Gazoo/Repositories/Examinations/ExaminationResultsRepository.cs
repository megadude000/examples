using Company.Gazoo.DbContexts;
using Microsoft.EntityFrameworkCore;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using System.Threading.Tasks;
using Company.Gazoo.Models.Examinations;
using AutoMapper;
using System.Linq;
using Company.Gazoo.Extensions;
using AutoMapper.QueryableExtensions;

namespace Company.Gazoo.Repositories.Examinations
{
    internal class ExaminationResultsRepository : IExaminationResultsRepository
    {
        private readonly IMapper mapper;
        private readonly GazooContext dbContext;
        private readonly DbSet<ExaminationResult> resultsDataSet;

        public ExaminationResultsRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;

            resultsDataSet = dbContext.Set<ExaminationResult>();
        }

        public Task AddAsync(ExaminationResult result)
        {
            resultsDataSet.Add(result);
            return dbContext.SaveChangesAsync();
        }

        public Task<ExaminationReportInfo[]> GetBarneyResultsAsync(ExaminationReportFilter filter)
            => resultsDataSet
                    .Include(report => report.Agent)
                    .Where(result => !result.Agent.LocalAgentId.HasValue)
                    .Where(result => filter.AgentsIds.Any() ? filter.AgentsIds.Any(selectedAgent => selectedAgent == result.AgentId) : true)
                    .Where(result => filter.ExaminationIds.Any(selectedExamId => selectedExamId == result.ExaminationId))
                    .Where(result => result.StartDate >= filter.FromDate)
                    .Where(result => result.EndDate <= filter.ToDate.SetToEndOfDay())
                    .Where(result => filter.InstanceIds.Any(selectedInstanceId => selectedInstanceId == result.InstanceId))
                    .Include(report => report.Examination)
                    .Include(report => report.Instance)
                    .ProjectTo<ExaminationReportInfo>(mapper.ConfigurationProvider)
                    .ToArrayAsync();

        public Task<ExaminationResult> GetAsync(long id)
            => resultsDataSet
                     .Where(result => result.Id == id)
                     .Include(report => report.Examination)
                     .Include(report => report.Agent)
                     .SingleOrDefaultAsync();

        public Task<ExaminationReportInfo[]> GetPebblesResultsAsync(ExaminationReportFilter filter)
            => resultsDataSet
                    .Include(report => report.Agent)
                    .Where(result => result.Agent.LocalAgentId.HasValue)
                    .Where(result => filter.AgentsIds.Any() ? filter.AgentsIds.Any(selectedAgent => selectedAgent == result.AgentId) : true)
                    .Where(result => filter.ExaminationIds.Any(selectedExamId => selectedExamId == result.ExaminationId))
                    .Where(result => result.StartDate >= filter.FromDate)
                    .Where(result => result.EndDate <= filter.ToDate.SetToEndOfDay())
                    .Where(result => filter.InstanceIds.Any(selectedInstanceId => selectedInstanceId == result.InstanceId))
                    .Include(report => report.Examination)
                    .Include(report => report.Instance)
                    .ProjectTo<ExaminationReportInfo>(mapper.ConfigurationProvider)
                    .ToArrayAsync();
    }
}