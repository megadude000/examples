using System.Threading.Tasks;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Company.Gazoo.Responses;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Company.Gazoo.Database.Enumerators;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Company.Gazoo.Models.Examinations.RequestModels;
using System.Collections.Generic;

namespace Company.Gazoo.Repositories.Examinations
{
    internal class ExaminationAnswersRepository: IExaminationAnswersRepository
    {
        private const long defaultAnswerSet = 1;

        private readonly IMapper mapper;
        private readonly GazooContext dbContext;
        private readonly DbSet<Answer> answersDataSet;

        public ExaminationAnswersRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;

            answersDataSet = dbContext.Set<Answer>();
        }

        public Task SoftDeleteAsync(long answerId)
        {
            var answerToDelete = answersDataSet.Single(answer => answer.Id == answerId);

            answerToDelete.IsDeleted = true;

            answersDataSet.Update(answerToDelete);
            return dbContext.SaveChangesAsync(); 
        }

        public Task AddAsync(Answer answer)
        {
            answersDataSet.Add(answer);
            return dbContext.SaveChangesAsync();
        }

        public Task<AnswerResponse[]> GetAsync(AnswerType type, long setId)
           => FilterByPredefinetSetId(setId)
                   .Where(answer => answer.Type == type)
                   .OrderByDescending(answer => answer.Id)
                   .ProjectTo<AnswerResponse>(mapper.ConfigurationProvider)
                   .ToArrayAsync();

        public Task<Answer[]> GetAllAsync(long setId)
            => FilterByPredefinetSetId(setId)
                    .Distinct()
                    .OrderByDescending(answer => answer.Id)
                    .ToArrayAsync();

        public Task<ExaminationAnswerModel[]> GetAsync(long setId)
            => FilterByPredefinetSetId(setId)
                    .Distinct()
                    .OrderByDescending(answer => answer.Id)
                    .ProjectTo<ExaminationAnswerModel>(mapper.ConfigurationProvider)
                    .ToArrayAsync();

        public Task<ExaminationAnswerModel[]> GetDefaultAnswersAsync()
             => GetNotDeleted()
                    .Distinct()
                    .Where(answer => answer.PredefinedAnswerSetId == defaultAnswerSet)
                    .OrderByDescending(answer => answer.Id)
                    .ProjectTo<ExaminationAnswerModel>(mapper.ConfigurationProvider)
                    .ToArrayAsync();

        public Task<bool> ExistsAsync(string name, AnswerType type, long setId)
            => FilterByPredefinetSetId(setId)
                    .Where(answer => answer.Type == type)
                    .AnyAsync(answer => answer.Name.ToUpper() == name.ToUpper());

        private IQueryable<Answer> FilterByPredefinetSetId(long setId)
            => GetNotDeleted()
                    .Where(answer => answer.PredefinedAnswerSetId == setId);

        private IQueryable<Answer> GetNotDeleted()
            => answersDataSet
                    .Where(answer => !answer.IsDeleted);
    }
}
