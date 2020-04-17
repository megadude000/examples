using AutoMapper;
using AutoMapper.QueryableExtensions;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Enumerators.Labeling;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Company.Gazoo.Responses.Labeling;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling
{
    internal class LabelGroupsRepository : ILabelGroupsRepository
    {
        private readonly IMapper mapper;
        private readonly DbContext dbContext;
        private readonly DbSet<LabelGroup> labelGroupsDataSet;

        public LabelGroupsRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;
            labelGroupsDataSet = dbContext.Set<LabelGroup>();
        }

        public Task AddAsync(LabelGroup audio)
        {
            labelGroupsDataSet.Add(audio);
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(LabelGroup labelGroup)
        {
            labelGroupsDataSet.Update(labelGroup);
            return dbContext.SaveChangesAsync();
        }

        public Task<LabelGroupResponse[]> GetAllAsync()
        {
            return labelGroupsDataSet
                .Where(labelGroup => !labelGroup.IsDeleted)
                .ProjectTo<LabelGroupResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();
        }

        public Task<LabelGroup> GetByIdAsync(long id)
        {
            return labelGroupsDataSet
                .Where(labelGroup => !labelGroup.IsDeleted)
                .FirstOrDefaultAsync(labelGroup => labelGroup.Id == id);
        }

        public Task<bool> CheckIfExistsAsync(string name)
        {
            return labelGroupsDataSet
                   .AnyAsync(label => label.Name.ToUpper() == name.ToUpper());
        }

        public Task<LabelElementType> GetElementTypeAsync(long labelGroupId)
            => labelGroupsDataSet
                .Where(labelGroup => !labelGroup.IsDeleted)
                .Where(labelGroup => labelGroup.Id == labelGroupId)
                .Select(labelGroup => labelGroup.Type)
                .SingleOrDefaultAsync();
    }
}
