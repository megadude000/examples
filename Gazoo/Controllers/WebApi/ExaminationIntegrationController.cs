namespace Company.Gazoo.Controllers.WebApi
{
    [Route("api/[controller]/[action]")]
    [Route("api/BarneyIntegration/[action]")]
    public class ExaminationIntegrationController : ControllerBase
    {
        private readonly IExaminationAnswersRepository answersRepository;
        private readonly IExaminationQuestionsRepository questionsRepository;
        private readonly IExaminationsRepository examinationsRepository;
        private readonly IExaminationIntegrationService examinationIntegrationService;
        private readonly IApplicationSubscriptionRepository applicationSubscriptionRepository;

        public ExaminationIntegrationController(IExaminationsRepository examinationsRepository,
            IExaminationAnswersRepository answersRepository,
            IExaminationQuestionsRepository questionsRepository,
            IExaminationIntegrationService examinationIntegrationService,
            IApplicationSubscriptionRepository applicationSubscriptionRepository)
        {
            this.answersRepository = answersRepository;
            this.questionsRepository = questionsRepository;
            this.examinationsRepository = examinationsRepository;
            this.examinationIntegrationService = examinationIntegrationService;
            this.applicationSubscriptionRepository = applicationSubscriptionRepository;
        }

        [HttpGet("{instanceName}")]
        public async Task<long> GetInstanceIdByName(string instanceName) 
            => await applicationSubscriptionRepository.GetIdByNameAsync(instanceName);

        [HttpGet]
        public async Task<ExaminationModel[]> GetExaminations() 
            => await examinationsRepository.GetModelAsync();

        [HttpGet("{examinationId}")]
        public async Task<ExaminationQuestionModel[]> GetExaminationData(long examinationId) 
            => await questionsRepository.GetModelRangeAsync(examinationId);

        [HttpGet]
        public async Task<ExaminationAnswerModel[]> GetExaminationAnswers()
           => await answersRepository.GetDefaultAnswersAsync();

        [HttpGet("{examinationId}")]
        public async Task<ExaminationAnswerModel[]> GetExaminationAnswers(long examinationId) 
            => await examinationIntegrationService.GetAnswersAsync(examinationId);

        [HttpPost]
        public async Task SavePebblesExaminationReports([FromBody] ExaminationReportsInformationModel report) 
            => await examinationIntegrationService.SaveReportAsync(report);
    }
}