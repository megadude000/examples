using AutoMapper;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Database.Enumerators;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Company.Gazoo.Requests;
using Company.Gazoo.Responses;
using Company.Gazoo.Services.Examinations.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations
{
    internal class ExaminationAnswersService : IExaminationAnswersService
    {
        private readonly IMapper mapper;
        private readonly IExaminationAnswersRepository answersRepository;
        private readonly IExaminationPredefinedAnswerSetsRepository predefinedAnswerSetsRepository;

        public ExaminationAnswersService(IMapper mapper,
            IExaminationAnswersRepository answersRepository,
            IExaminationPredefinedAnswerSetsRepository predefinedAnswerSetsRepository)
        {
            this.mapper = mapper;
            this.answersRepository = answersRepository;
            this.predefinedAnswerSetsRepository = predefinedAnswerSetsRepository;
        }

        public async Task AddAsync(AddAnswerRequest request)
            => await answersRepository.AddAsync(mapper.Map<Answer>(request));

        public async Task UpdateSetAsync(UpdateAnswerSetRequest request)
        {
            var answerSet = await predefinedAnswerSetsRepository.GetAsync(request.Id);

            answerSet.Name = request.Name;

            await predefinedAnswerSetsRepository.UpdateAsync(answerSet);
        }

        public async Task<AllAnswersResponse> GetAsync(long setId)
        {
            var groupedAnswers = (await answersRepository.GetAllAsync(setId))
                .GroupBy(answer => answer.Type)
                .ToDictionary(group => group.Key, group => group.ToArray());

            return new AllAnswersResponse
            {
                Scripts = MapAnswerToResponse(groupedAnswers, AnswerType.Script),
                Objections = MapAnswerToResponse(groupedAnswers, AnswerType.Objection),
                QuickAnswers = MapAnswerToResponse(groupedAnswers, AnswerType.QuickAnswer)
            };
        }

        private AnswerResponse[] MapAnswerToResponse(Dictionary<AnswerType, Answer[]> groupedAnswers, AnswerType type)
        {
            if (!groupedAnswers.Keys.Contains(type))
                return Array.Empty<AnswerResponse>();

            return mapper.Map<AnswerResponse[]>(groupedAnswers[type]);
        }
    }
}
