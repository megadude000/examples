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
    internal class ExaminationQuestionResultsRepository : IExaminationQuestionResultsRepository
    {
        private readonly IMapper mapper;
        private readonly GazooContext dbContext;
        private readonly DbSet<QuestionResult> questionResultsDataSet;
        private readonly DbSet<QuestionAnswer> questionAnswersDataSet;
        private readonly DbSet<QuestionResultAnswers> questionResultAnswersDataSet;

        public ExaminationQuestionResultsRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;

            questionResultsDataSet = dbContext.Set<QuestionResult>();
            questionAnswersDataSet = dbContext.Set<QuestionAnswer>();
            questionResultAnswersDataSet = dbContext.Set<QuestionResultAnswers>();
        }

        public Task AddResultAsync(QuestionResult questionResult)
        {
            questionResultsDataSet.Add(questionResult);
            return dbContext.SaveChangesAsync();
        }

        public Task AddResultAnswerAsync(QuestionResultAnswers questionResultAnswer)
        {
            questionResultAnswersDataSet.Add(questionResultAnswer);
            return dbContext.SaveChangesAsync();
        }

        public Task AddResultAnswerRangeAsync(QuestionResultAnswers[] questionResultAnswers)
        {
            questionResultAnswersDataSet.AddRange(questionResultAnswers);
            return dbContext.SaveChangesAsync();
        }

        public Task<QuestionResultResponse[]> GetResultsAsync(long reportId)
            => questionResultsDataSet
                .Include(item => item.Question)
                    .ThenInclude(item => item.QuestionAudioFile)
                    .Include(item => item.Question.QuestionAnswers)
                .Include(item => item.QuestionResultAnswers)
                    .ThenInclude(item => item.SelectedAnswer)
                .Where(item => item.ExaminationResultId == reportId)
                .ProjectTo<QuestionResultResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();
    }
}
