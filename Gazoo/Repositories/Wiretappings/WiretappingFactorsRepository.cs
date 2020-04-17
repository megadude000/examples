using AutoMapper;
using AutoMapper.QueryableExtensions;
using Company.BammBamm.Database.Contexts;
using Company.BammBamm.Database.Entities.Campaigns;
using Company.Gazoo.Models.Wiretapping;
using Company.Gazoo.Repositories.Wiretappings.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Wiretappings
{
    internal class WiretappingFactorsRepository : IWiretappingFactorsRepository
    {
        private readonly BammBammContext dbContext;
        private readonly DbSet<WiretappingFactor> wiretappingFactorDataSet;
        private readonly IMapper mapper;

        public WiretappingFactorsRepository(BammBammContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;
            wiretappingFactorDataSet = dbContext.Set<WiretappingFactor>();
        }

        public Task AddAsync(WiretappingFactor newImport)
        {
            wiretappingFactorDataSet.Add(newImport);

            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(WiretappingFactor import)
        {
            wiretappingFactorDataSet.Update(import);

            return dbContext.SaveChangesAsync();
        }

        public Task<WiretappingFactor> GetAsync(long id)
            => wiretappingFactorDataSet
                .SingleOrDefaultAsync(import => import.Id == id);

        public Task<bool> ExistsAsync(string factorName)
            => wiretappingFactorDataSet
                .AnyAsync(factor => factor.Name == factorName);

        public Task<WiretappingFactorModel[]> GetFactorsAsync()
            => wiretappingFactorDataSet
                .OrderByDescending(factor => factor.Id)
                .ProjectTo<WiretappingFactorModel>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        public Task<WiretappingFactorModel[]> GetEnabledFactorsAsync()
             => wiretappingFactorDataSet
                .Where(factor => factor.Enabled)
                .OrderByDescending(factor => factor.Id)
                .ProjectTo<WiretappingFactorModel>(mapper.ConfigurationProvider)
                .ToArrayAsync();
    }
}
