using AutoMapper;
using AutoMapper.QueryableExtensions;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Extensions;
using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Examinations
{
    internal class ExaminationStatisticsRepository : IExaminationStatisticsRepository
    {
        private IMapper mapper;
        private readonly DbSet<ExaminationStatistic> examinationStatisticsDataSet;

        public ExaminationStatisticsRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            examinationStatisticsDataSet = dbContext.Set<ExaminationStatistic>();
        }

        public Task<ExaminationStatisticModel[]> GetAsync(ExaminationStatisticsFilter filter)
        {
            return examinationStatisticsDataSet
                .FromSqlRaw("select * from examinations.get_examination_statistic({0}, {1}, {2}, {3}, {4}, {5})", 
                    filter.AgentsIds,
                    filter.ExaminationIds,
                    filter.InstanceIds,
                    filter.FromDate,
                    filter.ToDate.SetToEndOfDay(),
                    filter.Source)
                .ProjectTo<ExaminationStatisticModel>(mapper.ConfigurationProvider)
                .ToArrayAsync();
        }
    }
}
