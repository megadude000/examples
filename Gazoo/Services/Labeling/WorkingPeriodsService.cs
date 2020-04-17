using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Company.Gazoo.Services.Labeling.Interfaces;
using System;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling
{
    internal class WorkingPeriodsService : IWorkingPeriodsService
    {
        private readonly IWorkingPeriodsRepository workingPeriodsRepository;

        public WorkingPeriodsService(IWorkingPeriodsRepository workingPeriodsRepository)
        {
            this.workingPeriodsRepository = workingPeriodsRepository;
        }

        public async Task SaveWorkingPeriodAsync(WorkingPeriodFilter filter, long spentTime)
        {
            var savedTime = await workingPeriodsRepository.GetAsync(filter);

            if (savedTime != null)
            {
                savedTime.Duration += TimeSpan.FromMilliseconds(spentTime);
                await workingPeriodsRepository.UpdateAsync(savedTime);
                return;
            }

            var timeModeltoSave = new WorkingPeriod
            {
                AgentId = filter.AgentId,
                Date = DateTime.UtcNow.Date,
                Type = filter.Type,
                Action = filter.Action,
                Duration = TimeSpan.FromMilliseconds(spentTime)
            };

            await workingPeriodsRepository.AddAsync(timeModeltoSave);
        }
    }
}
