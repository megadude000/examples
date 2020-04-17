using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Database.Entities.Enums;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Company.Gazoo.Models.Labeling;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class WorkingPeriodsRepository : IWorkingPeriodsRepository
    {
        private readonly DbContext dbContext;
        private readonly DbSet<WorkingPeriod> workingPeriodsDataSet;

        public WorkingPeriodsRepository(GazooContext dbContext)
        {
            this.dbContext = dbContext;
            workingPeriodsDataSet = dbContext.Set<WorkingPeriod>();
        }
        public Task AddAsync(WorkingPeriod time)
        {
            workingPeriodsDataSet.Add(time);

            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(WorkingPeriod time)
        {
            workingPeriodsDataSet.Update(time);

            return dbContext.SaveChangesAsync();
        }

        public Task<WorkingPeriod> GetAsync(WorkingPeriodFilter filter)
        {
            return workingPeriodsDataSet
                .Where(period => period.AgentId == filter.AgentId)
                .Where(period => period.Date == DateTime.UtcNow.Date)
                .Where(period => period.Type == filter.Type)
                .Where(period => period.Action == filter.Action)
                .SingleOrDefaultAsync();
        }
    }
}
