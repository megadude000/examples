using AutoMapper;
using AutoMapper.QueryableExtensions;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Company.Gazoo.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Examinations
{
    internal class ExaminationPredefinedAnswerSetsRepository : IExaminationPredefinedAnswerSetsRepository
    {
        private readonly IMapper mapper;
        private readonly GazooContext dbContext;
        private readonly DbSet<PredefinedAnswerSet> predefinedAnswersDataSet;

        public ExaminationPredefinedAnswerSetsRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;

            predefinedAnswersDataSet = dbContext.Set<PredefinedAnswerSet>();
        }

        public Task AddAsync(PredefinedAnswerSet answerSet)
        {
            predefinedAnswersDataSet.Add(answerSet);

            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(PredefinedAnswerSet answerSet)
        {
            predefinedAnswersDataSet.Update(answerSet);

            return dbContext.SaveChangesAsync();
        }

        public Task DeleteAsync(long answerSetId)
        {
            predefinedAnswersDataSet.Remove(new PredefinedAnswerSet { Id = answerSetId });
            return dbContext.SaveChangesAsync();
        }

        public Task<PredefinedAnswerSetResponse[]> GetAsync()
            => predefinedAnswersDataSet
                .OrderByDescending(asnwerSet => asnwerSet.Id)
                .ProjectTo<PredefinedAnswerSetResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        public Task<PredefinedAnswerSet> GetAsync(long id)
             => predefinedAnswersDataSet
                .SingleOrDefaultAsync(asnwerSet => asnwerSet.Id == id);

        public Task<bool> ExistsAsync(string name)
            => predefinedAnswersDataSet.AnyAsync(asnwerSet => asnwerSet.Name == name);

        public Task<string> GetNameAsync(long answerSetId)
            => predefinedAnswersDataSet
                .Where(asnwerSet => asnwerSet.Id == answerSetId)
                .Select(asnwerSet => asnwerSet.Name)
                .SingleOrDefaultAsync();
    }
}
