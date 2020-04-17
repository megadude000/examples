using AutoMapper;
using Company.Database.Utils.Transaction.Interfaces;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Company.Gazoo.Requests;
using Company.Gazoo.Services.Examinations.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations
{
    internal class ExaminationQuestionsService : IExaminationQuestionsService
    {
        private readonly IMapper mapper;
        private readonly ITransactionService transactionService;
        private readonly IExaminationsService examinationsService;
        private readonly IExaminationQuestionsRepository questionsRepository;

        public ExaminationQuestionsService(IMapper mapper,
            ITransactionService transactionService,
            IExaminationsService examinationsService,
            IExaminationQuestionsRepository questionsRepository)
        {
            this.mapper = mapper;
            this.transactionService = transactionService;
            this.examinationsService = examinationsService;
            this.questionsRepository = questionsRepository;
        }

        public Task DeleteAsync(long questionId)
        {
            return transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                await questionsRepository.SoftDeleteAsync(questionId);
                var question = await questionsRepository.GetAsync(questionId);
                await examinationsService.UpdateModificationTimeAsync(question.ExaminationId);
            });
        }

        public Task AddAsync(AddQuestionRequest request)
        {
            return transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                var question = mapper.Map<Question>(request);
                await AddAsync(question, request.Answers);
                await examinationsService.UpdateModificationTimeAsync(question.ExaminationId);
            });
        }

        public Task UpdateAsync(UpdateQuestionRequest request)
        {
            return transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                await questionsRepository.SoftDeleteAsync(request.QuestionId);

                var question = mapper.Map<Question>(request);
                await AddAsync(question, request.Answers);
                await examinationsService.UpdateModificationTimeAsync(question.ExaminationId);
            });
        }

        private async Task AddAsync(Question question, AnswerModel[] answers)
        {
            await questionsRepository.AddAsync(question);
            await questionsRepository.AddQuestionAnswersRangeAsync(CreateQuestionAnswerCollection(question.Id, answers));
        }

        private QuestionAnswer[] CreateQuestionAnswerCollection(long questionId, AnswerModel[] answers)
        {
            List<QuestionAnswer> result = new List<QuestionAnswer>();

            foreach (var answer in answers)
            {
                if (answer.NextAnswers != null)
                {
                    Answer prevAnswer = null;
                    foreach (var nextAnswer in answer.NextAnswers)
                    {
                        if (prevAnswer == null)
                            AddAnswerToCollection(ref result, answer.Id, questionId, nextAnswer.Id, IsHead: true);
                        else
                            AddAnswerToCollection(ref result, prevAnswer.Id, questionId, nextAnswer.Id, IsHead: false);

                        prevAnswer = nextAnswer;
                    }
                    AddAnswerToCollection(ref result, prevAnswer.Id, questionId, null, IsHead: false);
                }
                else
                {
                    AddAnswerToCollection(ref result, answer.Id, questionId, null, IsHead: true);
                }
            }
                
            return result.ToArray();
        }

        private void AddAnswerToCollection(ref List<QuestionAnswer> result, long answerId, long questionId, long? nextAnswerId, bool IsHead)
        {
            result.Add(new QuestionAnswer
            {
                AnswerId = answerId,
                QuestionId = questionId,
                NextAnswerId = nextAnswerId,
                IsHead = IsHead
            });
        }
    }
}
