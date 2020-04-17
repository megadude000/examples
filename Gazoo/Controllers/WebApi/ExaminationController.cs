using Company.AspNet.Identity.Entities;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Database.Enumerators;
using Company.Gazoo.Extensions;
using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Company.Gazoo.Requests;
using Company.Gazoo.Responses;
using Company.Gazoo.Services.Examinations.Interfaces;
using Company.Gazoo.Services.Examinations.Strategies.Interfaces;
using Company.Gazoo.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Company.Gazoo.Controllers.WebApi
{
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = CustomPolicies.ExaminationMasterOrViewer)]
    public class ExaminationController : ControllerBase
    {
        private readonly UserManager<CompanyIdentityUser> userManager;
        private readonly IExaminationAnswersService answersService;
        private readonly IExaminationQuestionsService questionsService;
        private readonly IExaminationAnswersRepository answersRepository;
        private readonly IExaminationQuestionsRepository questionsRepository;
        private readonly IExaminationsService examinationsService;
        private readonly IExaminationAudioService examinationAudioService;
        private readonly IExaminationReadStrategy examinationReadStrategy;
        private readonly IExaminationsRepository examinationsRepository;
        private readonly IExaminationQuestionAudioFilesRepository questionAudioFilesRepository;
        private readonly IExaminationPredefinedAnswerSetsRepository predefinedAnswerSetsRepository;
        private readonly IExaminationStatisticsRepository examinationStatisticsRepository;

        public ExaminationController(UserManager<CompanyIdentityUser> userManager,
            IExaminationAnswersService answersService,
            IExaminationQuestionsService questionsService,
            IExaminationAnswersRepository answersRepository,
            IExaminationQuestionsRepository questionsRepository,
            IExaminationsService examinationsService,
            IExaminationAudioService examinationAudioService,
            IExaminationReadStrategy examinationReadStrategy,
            IExaminationsRepository examinationsRepository,
            IExaminationQuestionAudioFilesRepository questionAudioFilesRepository,
            IExaminationPredefinedAnswerSetsRepository predefinedAnswerSetsRepository,
            IExaminationStatisticsRepository examinationStatisticsRepository)
        {
            this.userManager = userManager;
            this.answersService = answersService;
            this.questionsService = questionsService;
            this.answersRepository = answersRepository;
            this.questionsRepository = questionsRepository;
            this.examinationsService = examinationsService;
            this.examinationAudioService = examinationAudioService;
            this.examinationReadStrategy = examinationReadStrategy;
            this.examinationsRepository = examinationsRepository;
            this.questionAudioFilesRepository = questionAudioFilesRepository;
            this.predefinedAnswerSetsRepository = predefinedAnswerSetsRepository;
            this.examinationStatisticsRepository = examinationStatisticsRepository;
        }

        [HttpPost]
        public async Task<ExaminationResponse[]> GetFilteredExaminations([FromBody] ExaminationFilterModel filter)
            => await examinationsRepository.GetAsync(filter);

        [HttpGet]
        public async Task<InstanceResponse[]> GetAvailableInstancesToUser()
            => await examinationsService.GetAvailableInstancesToUserAsync(User);

        [HttpPost]
        public async Task DeleteExamination([FromBody] long examId)
            => await examinationsService.DeleteAsync(examId);

        public async Task AddExamination([FromBody] AddExaminationRequest request)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            await examinationsService.AddAsync(request, user.Id);
        }

        [HttpGet("{examTitle}")]
        public async Task<bool> CheckExaminationNameExists([FromRoute] string examTitle)
            => await examinationsRepository.NameExistsAsync(examTitle);

        [HttpPost]
        public async Task<ExaminationInfoResponse> GetExaminationInfo([FromBody] long examId)
            => await examinationsRepository.GetExaminationInfoResponseAsync(examId);

        [HttpPost]
        public async Task UpdateExamination([FromBody] UpdateExaminationRequest request)
            => await examinationsService.UpdateAsync(request);

        [HttpPost]
        public async Task ChangeRandomState([FromBody] long examinationId)
            => await examinationsService.ChangeRandomStateAsync(examinationId);

        [HttpPost]
        public async Task<ExaminationForReportResponse[]> GetExaminationsForReports()
            => await examinationsRepository.GetForReportsAsync();

        [HttpPost]
        public async Task<AgentForReportResponse[]> GetAgentsForReports([FromBody] SearchRequest searchRequest)
            => await examinationReadStrategy.GetAgentsForReportsAsync(searchRequest);

        [HttpPost]
        public async Task<ExaminationReportInfo[]> GetExaminationReports([FromBody] ExaminationReportFilter filter)
            => await examinationReadStrategy.GetReportsAsync(filter);

        [HttpGet("{reportId}")]
        public async Task<ExaminationReportResponse> GetExaminationReport([FromRoute] long reportId)
            => await examinationsService.GetReportAsync(reportId);

        [HttpPost]
        public async Task<ExaminationStatisticModel[]> GetExaminationStatistics([FromBody] ExaminationStatisticsFilter filter)
            => await examinationStatisticsRepository.GetAsync(filter);

        [HttpGet("{answerSetId}")]
        public async Task<bool> CheckAnswerSetIsUsed(long answerSetId)
            => await examinationsRepository.AnswerSetIsUsedAsync(answerSetId);

        [HttpGet]
        public async Task<PredefinedAnswerSetResponse[]> GetPredefinedAnswerSets()
            => await predefinedAnswerSetsRepository.GetAsync();

        [HttpGet("{name}")]
        public async Task<bool> CheckAnswerSetNameExists(string name)
            => await predefinedAnswerSetsRepository.ExistsAsync(name);

        [HttpPost]
        public async Task AddAnswerSet([FromBody] AddAnswerSetRequest request)
            => await predefinedAnswerSetsRepository.AddAsync(new PredefinedAnswerSet { Name = request.Name });

        [HttpPost]
        public async Task UpdateAnswerSet([FromBody] UpdateAnswerSetRequest request)
            => await answersService.UpdateSetAsync(request);

        [HttpGet("{answerSetId}")]
        public async Task<string> GetPredefinedAnswerSetName(long answerSetId)
            => await predefinedAnswerSetsRepository.GetNameAsync(answerSetId);

        [HttpGet("{answerId}")]
        public async Task<bool> CheckAnswerIsUsed(long answerId)
            => await questionsRepository.AnswerIsUsedAsync(answerId);

        [HttpGet("{setId}")]
        public async Task<AllAnswersResponse> GetAllAnswers(long setId)
             => await answersService.GetAsync(setId);

        [HttpGet("{type}/{setId}")]
        public async Task<AnswerResponse[]> GetAnswers(AnswerType type, long setId)
            => await answersRepository.GetAsync(type, setId);

        [HttpPost]
        public async Task DeleteAnswer([FromBody] long answerId)
            => await answersRepository.SoftDeleteAsync(answerId);

        [HttpPost]
        public async Task DeleteAnswerSet([FromBody] long answerSetId)
            => await predefinedAnswerSetsRepository.DeleteAsync(answerSetId);

        [HttpPost]
        public async Task<bool> CheckAnswerExists([FromBody] AnswerExistsRequest request)
            => await answersRepository.ExistsAsync(request.Name, request.Type, request.SetId);

        [HttpPost]
        public async Task AddAnswer([FromBody] AddAnswerRequest request)
            => await answersService.AddAsync(request);

        [HttpPost]
        public async Task<QuestionResponse[]> GetExaminationQuestions([FromBody] long examId)
             => await questionsRepository.GetResponseRangeAsync(examId);

        [HttpPost]
        public async Task DeleteQuestion([FromBody] long questionId)
            => await questionsService.DeleteAsync(questionId);

        [HttpPost]
        public async Task AddQuestion([FromBody] AddQuestionRequest question)
            => await questionsService.AddAsync(question);

        [HttpPost]
        public async Task UpdateQuestion([FromBody] UpdateQuestionRequest request)
            => await questionsService.UpdateAsync(request);

        [HttpGet]
        public async Task<AudioFileResponse[]> GetAllExaminationAudios() 
            => await questionAudioFilesRepository.GetAllAsync();

        [HttpPost]
        public async Task<DeleteExaminationAudioResult> DeleteSelecedAudio([FromBody] long[] filesIds) 
            => await examinationAudioService.DeleteAsync(filesIds);

        [HttpGet("{fileName}")]
        public async Task<bool> CheckIfFileExist(string fileName) 
            => await questionAudioFilesRepository.ExistsAsync(fileName);

        [HttpPost]
        public async Task<bool> UpdateAudioFileInfo([FromBody] UpdateExaminationAudioFileInfoRequest request)
            => await examinationAudioService.UpdateAsync(request);

        [HttpGet("{id}")]
        public async Task<FileResult> GetAudioFile([FromRoute] long id)
        {
            var source = await examinationAudioService.GetAsync(id);

            if (source == null)
                return null;

            source.Position = 0;
            return File(source, "audio/wav");
        }

        [HttpPost]
        public async Task<SaveExaminationAudioResult> SaveExaminationAudio()
        {
            var boundary = MultipartRequestHelper.GetBoundary(Request.ContentType);
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType) || boundary == null)
                return SaveExaminationAudioResult.Error;

            return await examinationAudioService.SaveAsync(HttpContext.Request.Body, boundary);
        }
    }
}