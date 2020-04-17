using AutoMapper;
using Company.Database.Utils.Transaction.Interfaces;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Models.Examinations.RequestModels;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Company.Gazoo.Services.Examinations.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations
{
    internal class ExaminationIntegrationService : IExaminationIntegrationService
    {
        private const long agentNotFound = 0;

        private readonly IMapper mapper;
        private readonly ITransactionService transactionService;
        private readonly IExaminationsRepository examinationsRepository;
        private readonly IExaminationAnswersRepository answersRepository;
        private readonly ILogger<ExaminationIntegrationService> logger;
        private readonly IExaminationQuestionResultsRepository questionResultsRepository;
        private readonly IExaminationAgentsRepository examinationAgentsRepository;
        private readonly IExaminationResultsRepository examinationResultsRepository;

        public ExaminationIntegrationService(IMapper mapper,
            IExaminationAnswersRepository answersRepository,
            ITransactionService transactionService,
            IExaminationsRepository examinationsRepository,
            ILogger<ExaminationIntegrationService> logger,
            IExaminationQuestionResultsRepository questionResultsRepository,
            IExaminationAgentsRepository examinationAgentsRepository,
            IExaminationResultsRepository examinationResultsRepository)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.answersRepository = answersRepository;
            this.transactionService = transactionService;
            this.examinationsRepository = examinationsRepository;
            this.questionResultsRepository = questionResultsRepository;
            this.examinationAgentsRepository = examinationAgentsRepository;
            this.examinationResultsRepository = examinationResultsRepository;
        }

        public async Task<ExaminationAnswerModel[]> GetAnswersAsync(long examinationId)
        {
            var predefinedAnswerSetId = await examinationsRepository.GetAnswerSetIdAsync(examinationId);

            return await answersRepository.GetAsync(predefinedAnswerSetId);
        }

        public async Task SaveReportAsync(ExaminationReportsInformationModel report)
        {
            try
            {
                var result = mapper.Map<ExaminationResult>(report);
                var agent = mapper.Map<Agent>(report);

                await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
                {
                    result.AgentId = await GetOrAddAgent(agent);
                    await examinationResultsRepository.AddAsync(result);
                    await SaveExaminationQuestionResults(result.Id, report.ExaminationReports);
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Error occured while saving examination report from pebbles", ex);
            }
        }

        private async Task<long> GetOrAddAgent(Agent agent)
        {
            var agentId = await examinationAgentsRepository.GetPebblesAgentIdAsync(agent.InstanceId.Value, agent.LocalAgentId.Value);
            if (agentId == agentNotFound)
            {
                await examinationAgentsRepository.AddAsync(agent);
                return agent.Id;
            }

            return agentId;
        }

        private async Task SaveExaminationQuestionResults(long resultId, ExaminationReportModel[] reports)
        {
            foreach (var qResult in reports)
            {
                var questonResult = new QuestionResult
                {
                    ExaminationResultId = resultId,
                    QuestionId = qResult.QuestionId,
                    ReactionTime = qResult.SpentTime,
                    Result = (int)qResult.Status
                };

                await questionResultsRepository.AddResultAsync(questonResult);

                if (qResult.Answer.Id != 0)
                {
                    await questionResultsRepository.AddResultAnswerAsync(new QuestionResultAnswers
                    {
                        QuestionResultId = questonResult.Id,
                        SelectedAnswerId = qResult.Answer.Id,
                        OrderNumber = 1
                    });

                    if (qResult.Answer.NextAnswers != null)
                    {
                        var order = 1;
                        var questionResultAnswers = qResult.Answer.NextAnswers
                            .Select(answer => new QuestionResultAnswers
                            {
                                QuestionResultId = questonResult.Id,
                                SelectedAnswerId = answer.Id,
                                OrderNumber = ++order
                            })
                            .ToArray();

                        await questionResultsRepository.AddResultAnswerRangeAsync(questionResultAnswers);
                    }
                }
            }
        }
    }
}
