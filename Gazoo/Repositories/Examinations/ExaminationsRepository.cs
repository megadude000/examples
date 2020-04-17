using Company.Gazoo.DbContexts;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Company.Gazoo.Models.Examinations;
using AutoMapper;
using System.Linq;
using Company.Gazoo.Responses;
using AutoMapper.QueryableExtensions;
using System;

namespace Company.Gazoo.Repositories.Examinations
{
    internal class ExaminationsRepository: IExaminationsRepository
    {
        private readonly IMapper mapper;
        private readonly GazooContext dbContext;
        private readonly DbSet<Examination> examinationDataSet;
        private readonly DbSet<Question> questionDataSet;

        public ExaminationsRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;

            examinationDataSet = dbContext.Set<Examination>();
            questionDataSet = dbContext.Set<Question>();
        }

        public Task AddAsync(Examination examination)
        {
            examinationDataSet.Add(examination);
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(Examination examination)
        {
            examinationDataSet.Update(examination);
            return dbContext.SaveChangesAsync();
        }

        public Task SoftDeleteAsync(long examId)
        {
            var examinationToDelete = examinationDataSet.Single(exam => exam.Id == examId);

            examinationToDelete.IsDeleted = true;
            examinationToDelete.ModificationTime = DateTime.UtcNow;

            examinationDataSet.Update(examinationToDelete);
            return dbContext.SaveChangesAsync();
        }

        public Task<ExaminationForReportResponse[]> GetForReportsAsync()
            => GetNotDeleted()
                .ProjectTo<ExaminationForReportResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        public Task<ExaminationResponse[]> GetAsync(ExaminationFilterModel filterModel)
           => GetNotDeleted()
               .Where(examination => examination.Name.ToUpper().Contains(filterModel.ExaminationTitle.ToUpper()))
               .Where(examination => filterModel.StartDate.HasValue ? examination.CreationTime.Date >= filterModel.StartDate.Value : examination.CreationTime.Date >= DateTime.MinValue)
               .Where(examination => filterModel.EndDate.HasValue ? examination.CreationTime.Date <= filterModel.EndDate.Value : examination.CreationTime.Date <= DateTime.MaxValue)
               .Include(examination => examination.Author)
                   .ThenInclude(author => author.Claims)
               .ProjectTo<ExaminationResponse>(mapper.ConfigurationProvider)
               .ToArrayAsync();

        public Task<ExaminationModel[]> GetModelAsync()
            => examinationDataSet
                .Include(examination => examination.Questions)
                .Where(examination => !examination.Questions.All(question => question.IsDeleted))
                .Distinct()
                .ProjectTo<ExaminationModel>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        public Task<bool> NameExistsAsync(string examTitle)
            => GetNotDeleted()
                .AnyAsync(examination => examination.Name.ToUpper() == examTitle.ToUpper());

        public Task<Examination> GetAsync(long examId)
            => examinationDataSet
                .SingleOrDefaultAsync(examination => examination.Id == examId);

        public Task<ExaminationInfoResponse> GetExaminationInfoResponseAsync(long examId)
            => examinationDataSet
                .Where(examination => examination.Id == examId)
                .ProjectTo<ExaminationInfoResponse>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

        public Task<bool> AnswerSetIsUsedAsync(long answerSetId)
            => GetNotDeleted()
                .AnyAsync(examination => examination.PredefinedAnswerSetId == answerSetId);

        public Task<long> GetAnswerSetIdAsync(long examId)
            => examinationDataSet
                .Where(examination => examination.Id == examId)
                .Select(examination => examination.PredefinedAnswerSetId)
                .SingleOrDefaultAsync();

        private IQueryable<Examination> GetNotDeleted()
            => examinationDataSet
                .Where(examination => !examination.IsDeleted);
    }
}
