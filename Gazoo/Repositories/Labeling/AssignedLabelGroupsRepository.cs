using AutoMapper;
using AutoMapper.QueryableExtensions;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class AssignedLabelGroupsRepository : IAssignedLabelGroupsRepository
    {
        private readonly IMapper mapper;
        private readonly DbContext dbContext;
        private readonly DbSet<AssignedLabelGroups> assignedLabelGroupsDataSet;

        public AssignedLabelGroupsRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;
            assignedLabelGroupsDataSet = dbContext.Set<AssignedLabelGroups>();
        }

        public Task AddRangeAsync(AssignedLabelGroups[] assignedLabels)
        {
            assignedLabelGroupsDataSet.AddRange(assignedLabels);

            return dbContext.SaveChangesAsync();
        }

        public Task<LabelGroupModel[]> GetAsync(long importNumber)
        {
            return assignedLabelGroupsDataSet
                  .Where(item => item.ImportNumber == importNumber)
                  .Select(item => item.LabelGroup)
                  .ProjectTo<LabelGroupModel>(mapper.ConfigurationProvider)
                  .ToArrayAsync();
        }
    }
}
