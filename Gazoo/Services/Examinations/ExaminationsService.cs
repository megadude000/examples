using AutoMapper;
using Company.Database.Utils.Transaction.Interfaces;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Extensions;
using Company.Gazoo.Repositories.ApplicationSubscriptions.Interfaces;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Company.Gazoo.Repositories.Users.Interfaces;
using Company.Gazoo.Requests;
using Company.Gazoo.Responses;
using Company.Gazoo.Services.Examinations.Interfaces;
using Company.Gazoo.Services.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations
{
    internal class ExaminationsService : IExaminationsService
    {
        private readonly IMapper mapper;
        private readonly ITransactionService transactionService;
        private readonly IExaminationQuestionsRepository questionsRepository;
        private readonly IUsersManagerRepository usersManagerRepository;
        private readonly IExaminationsRepository examinationsRepository;
        private readonly IExaminationQuestionResultsRepository questionResultsRepository;
        private readonly IExaminationResultsRepository examinationResultsRepository;
        private readonly IApplicationSubscriptionRepository applicationSubscriptionRepository;
        private readonly IUserInstancesAccessPermissionRepository userInstancesAccessPermissionRepository;

        public ExaminationsService(IMapper mapper,
            ITransactionService transactionService,
            IExaminationQuestionsRepository questionsRepository,
            IExaminationsRepository examinationsRepository,
            IUsersManagerRepository usersManagerRepository,
            IExaminationQuestionResultsRepository questionResultsRepository,
            IExaminationResultsRepository examinationResultsRepository,
            IApplicationSubscriptionRepository applicationSubscriptionRepository,
            IUserInstancesAccessPermissionRepository userInstancesAccessPermissionRepository)
        {
            this.mapper = mapper;
            this.transactionService = transactionService;
            this.questionsRepository = questionsRepository;
            this.examinationsRepository = examinationsRepository;
            this.usersManagerRepository = usersManagerRepository;
            this.questionResultsRepository = questionResultsRepository;
            this.examinationResultsRepository = examinationResultsRepository;
            this.applicationSubscriptionRepository = applicationSubscriptionRepository;
            this.userInstancesAccessPermissionRepository = userInstancesAccessPermissionRepository;
        }

        public Task AddAsync(AddExaminationRequest request, long userId)
        {
            var examination = mapper.Map<Examination>(request);

            examination.AuthorId = userId;

            return examinationsRepository.AddAsync(examination);
        }

        public Task DeleteAsync(long examId)
        {
            return transactionService.CommitAsync(new[] { TransactionContextScope.BammBamm }, async () =>
            {
                await examinationsRepository.SoftDeleteAsync(examId);
                await questionsRepository.SoftDeleteRangeAsync(examId);
            });
        }

        public async Task UpdateAsync(UpdateExaminationRequest request)
        {
            var exam = await examinationsRepository.GetAsync(request.Id);

            exam.Name = request.Name;
            exam.ReactionTime = request.ReactionTime;
            exam.ModificationTime = DateTime.UtcNow;

            await examinationsRepository.UpdateAsync(exam);
        }

        public Task ChangeRandomStateAsync(long examinationId)
        {
            return transactionService.CommitAsync(new[] { TransactionContextScope.BammBamm }, async () =>
            {
                var exam = await examinationsRepository.GetAsync(examinationId);

                exam.IsRandom = !exam.IsRandom;

                await examinationsRepository.UpdateAsync(exam);
                await UpdateModificationTimeAsync(examinationId);
            });
        }

        public async Task<InstanceResponse[]> GetAvailableInstancesToUserAsync(ClaimsPrincipal user)
        {
            if (user.HasClaim(claim => claim.Type == CustomClaims.ExamMaster))
                return await applicationSubscriptionRepository.GetExaminationInstancesAsync();

            var userInfo = await usersManagerRepository.FindByNameAsync(user.Identity.Name);

            return await userInstancesAccessPermissionRepository.GetAvailableInstancesToUserAsync(userInfo.Id);
        }

        public async Task<ExaminationReportResponse> GetReportAsync(long reportId)
        {
            var examResult = await examinationResultsRepository.GetAsync(reportId);
            var questionResultsResponse = await questionResultsRepository.GetResultsAsync(reportId);

            return new ExaminationReportResponse
            {
                ExaminationResult = examResult,
                QuestionResults = questionResultsResponse
            };
        }

        public async Task UpdateModificationTimeAsync(long examinationId)
        {
            var examination = await examinationsRepository.GetAsync(examinationId);

            examination.ModificationTime = DateTime.UtcNow;

            await examinationsRepository.UpdateAsync(examination);
        }
    }
}
