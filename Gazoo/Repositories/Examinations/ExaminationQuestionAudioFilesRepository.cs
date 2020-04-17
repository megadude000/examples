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
    internal class ExaminationQuestionAudioFilesRepository : IExaminationQuestionAudioFilesRepository
    {
        private readonly IMapper mapper;
        private readonly DbContext dbContext;
        private readonly DbSet<QuestionAudioFile> audiosDataSet;

        public ExaminationQuestionAudioFilesRepository(GazooContext dbContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;

            audiosDataSet = dbContext.Set<QuestionAudioFile>();
        }

        public Task UpdateAsync(QuestionAudioFile audioFile)
        {
            audiosDataSet.Update(audioFile);
            return dbContext.SaveChangesAsync();
        }

        public Task AddAsync(QuestionAudioFile audioFile)
        {
            audiosDataSet.Add(audioFile);
            return dbContext.SaveChangesAsync();
        }

        public Task SoftDeleteRangeAsync(long[] filesIds)
        {
            var audiosToSoftDelete = audiosDataSet.Where(audio => filesIds.Contains(audio.Id));

            foreach (var audio in audiosToSoftDelete)
                audio.IsDeleted = true;

            audiosDataSet.UpdateRange(audiosToSoftDelete);
            return dbContext.SaveChangesAsync();
        }

        public Task<AudioFileResponse[]> GetAllAsync()
            => GetNotDeleted()
                .ProjectTo<AudioFileResponse>(mapper.ConfigurationProvider)
                .ToArrayAsync();

        public Task<QuestionAudioFile> GetAsync(long id)
            => audiosDataSet
                .FirstOrDefaultAsync(audio => audio.Id == id);

        public Task<bool> ExistsAsync(string fileName)
            => GetNotDeleted()
                .AnyAsync(audio => EF.Functions.ILike(audio.Name, fileName));

        public Task<string[]> GetNamesAsync()
            => GetNotDeleted()
                .Select(item => item.Name)
                .ToArrayAsync();

        public Task<QuestionAudioFile[]> GetRangeByNamesAsync(string[] fileNames)
            => audiosDataSet
                .Where(audio => fileNames.Contains(audio.Name))
                .ToArrayAsync();

        public Task<string[]> GetNamesRangeAsync(long[] filesIds)
           => audiosDataSet
               .Where(audio => filesIds.Contains(audio.Id))
               .Select(audio => audio.Name)
               .ToArrayAsync();

        private IQueryable<QuestionAudioFile> GetNotDeleted()
            => audiosDataSet
                .Where(audio => !audio.IsDeleted);
    }
}
