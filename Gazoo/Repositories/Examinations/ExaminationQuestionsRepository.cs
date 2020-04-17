using AutoMapper;
using AutoMapper.QueryableExtensions;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Examinations
{
    internal class ExaminationQuestionsRepository: IExaminationQuestionsRepository
    {
        private readonly IMapper mapper;
        private readonly GazooContext dbContext;
        private readonly DbSet<Question> questionsDataSet;
        private readonly DbSet<QuestionAnswer> questionAnswersDataSet;

        public ExaminationQuestionsRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;

            questionsDataSet = dbContext.Set<Question>();
            questionAnswersDataSet = dbContext.Set<QuestionAnswer>();
        }

        public Task UpdateAsync(Question question)
        {
            questionsDataSet.Update(question);
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateRangeAsync(Question[] questions)
        {
            questionsDataSet.UpdateRange(questions);
            return dbContext.SaveChangesAsync();
        }

        public Task SoftDeleteAsync(long questionId)
        {
            var questionToDelete = questionsDataSet.Single(question => question.Id == questionId);

            questionToDelete.IsDeleted = true;

            questionsDataSet.Update(questionToDelete);
            return dbContext.SaveChangesAsync();
        }

        public Task AddAsync(Question question)
        {
            questionsDataSet.Add(question);
            return dbContext.SaveChangesAsync();
        }

        public Task AddQuestionAnswersRangeAsync(QuestionAnswer[] questionAnswers)
        {
            questionAnswersDataSet.AddRange(questionAnswers);
            return dbContext.SaveChangesAsync();
        }

        public Task SoftDeleteRangeAsync(long examId)
        {
            var questionsToDelete = questionsDataSet.Where(question => question.Examination.Id == examId);

            if (!questionsToDelete.Any())
                return Task.CompletedTask;

            foreach (var question in questionsToDelete)
                question.IsDeleted = true;

            questionsDataSet.UpdateRange(questionsToDelete);

            return dbContext.SaveChangesAsync();
        }

        public Task<bool> AnswerIsUsedAsync(long answerId) 
            => questionAnswersDataSet
                .AnyAsync(entity => entity.AnswerId == answerId && !entity.Question.IsDeleted);

        public Task<Question> GetAsync(long questionId) 
            => questionsDataSet
                .FirstOrDefaultAsync(entity => entity.Id == questionId);

        public Task<bool> AudioIsUsedAsync(long[] filesIds) 
            => GetNotDeleted()
                .AnyAsync(audio => filesIds.Any(fileId => fileId == audio.QuestionAudioFileId));

        public Task<ExaminationQuestionModel[]> GetModelRangeAsync(long examId)
            => GetByExaminationId(examId)
                .ProjectTo<ExaminationQuestionModel>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        public Task<QuestionResponse[]> GetResponseRangeAsync(long examId)
            => GetByExaminationId(examId)
                .ProjectTo<QuestionResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        private IQueryable<Question> GetByExaminationId(long examId)
            => GetNotDeleted()
                .AsNoTracking()
                .Include(entity => entity.QuestionAudioFile)
                .Include(entity => entity.Examination)
                .Where(item => !item.QuestionAudioFile.IsDeleted)
                .Where(item => item.ExaminationId == examId)
                .Include(entity => entity.QuestionAnswers)
                   .ThenInclude(entity => entity.Answer);

        private IQueryable<Question> GetNotDeleted()
            => questionsDataSet
                .Where(question => !question.IsDeleted);
    }
}
