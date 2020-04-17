namespace Company.Gazoo.Controllers.WebApi
{
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = CustomPolicies.ConversationTranscriberOrMaster)]
    public class TranscriptionController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly UserManager<CompanyIdentityUser> userManager;
        private readonly ITranscriptionService transcriptionService;
        private readonly ILabelingAudioService labelingAudioService;
        private readonly ITranscriptionRepository transcriptionRepository;
        private readonly IImportRepository importsRepository;
        private readonly ILabelService labelService;
        private readonly ITranscriptionLabelsRepository labelsRepository;
        private readonly ILabelGroupsRepository labelGroupsRepository;
        private readonly ILabelingStatisticsService labelingStatisticsService;

        public TranscriptionController(ITranscriptionService transcriptionService,
            UserManager<CompanyIdentityUser> userManager,
            IMapper mapper,
            ILabelingAudioService labelingAudioService,
            ILabelService labelService,
            IImportRepository importsRepository,
            ITranscriptionRepository transcriptionRepository,
            ILabelingStatisticsService labelingStatisticsService,
            ITranscriptionLabelsRepository labelsRepository,
            ILabelGroupsRepository labelGroupsRepository)
        {
            this.mapper = mapper;
            this.labelGroupsRepository = labelGroupsRepository;
            this.labelsRepository = labelsRepository;
            this.importsRepository = importsRepository;
            this.transcriptionRepository = transcriptionRepository;
            this.labelingStatisticsService = labelingStatisticsService;
            this.labelingAudioService = labelingAudioService;
            this.transcriptionService = transcriptionService;
            this.labelService = labelService;
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<UserModel[]> GetAgentsForReports([FromBody] SearchRequest searchRequest)
            => await labelingStatisticsService.GetAgentsForReportsAsync(searchRequest);

        [HttpPost]
        public async Task<GeneralLabelingStatisticResponse> GetGeneralStatistics([FromBody] LabelingStatisticRequest request)
            => await labelingStatisticsService.GetGeneralStatisticsAsync(request);

        [HttpPost]
        public async Task<TranscriptionStatisticsResponse> GetTranscriptionStatistics([FromBody] LabelingStatisticRequest request)
            => await labelingStatisticsService.GetTranscriptionStatisticsAsync(request);

        [HttpPost]
        public async Task<FCMomentStatisticsResponse> GetMomentsPredictionStatistics([FromBody] LabelingStatisticRequest request)
           => await labelingStatisticsService.GetFCMomentStatisticsAsync(request);

        [HttpGet("{labelGroupName}")]
        public async Task<bool> CheckLabelGroupNameExists(string labelGroupName)
            => await labelGroupsRepository.CheckIfExistsAsync(labelGroupName);

        [HttpPost]
        public async Task UpdateLabelGroup([FromBody] UpdateLabelGroupRequest request)
            => await labelService.UpdateLabelGroupAsync(request);

        [HttpPost]
        public async Task AddLabelGroup([FromBody] AddLabelGroupRequest request)
            => await labelService.AddLabelGroupAsync(request);

        [HttpPost]
        public async Task DeleteLabelGroup([FromBody] long labelGroupId)
            => await labelService.SoftDeleteLabelGroupAsync(labelGroupId);

        [HttpGet]
        public async Task<LabelGroupResponse[]> GetLabelGroups()
            => await labelGroupsRepository.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<TranscriptionLabelModel[]> GetLabels(long id)
            => await labelService.GetTranscriptionLabelsAsync(id);

        [HttpGet("{labelName}/{groupId}")]
        public async Task<bool> CheckLabelNameExists(string labelName, long groupId)
            => await labelsRepository.CheckIfExistsAsync(labelName, groupId);

        [HttpPost]
        public async Task AddLabel([FromBody] AddTranscriptionLabelRequest request)
            => await labelsRepository.AddAsync(mapper.Map<TranscriptionLabel>(request));

        [HttpPost]
        public async Task DeleteLabel([FromBody] long labelId)
            => await labelService.SoftDeleteLabelAsync(labelId);

        [HttpPost]
        public async Task UpdateLabel([FromBody] UpdateTranscriptionLabelRequest request)
            => await labelsRepository.UpdateAsync(mapper.Map<TranscriptionLabel>(request));

        [HttpPost]
        public async Task SaveAudioTrancription([FromBody]SaveTranscriptionRequest request)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            await transcriptionService.SaveAudioTrancriptionAsync(request, user.Id);
        }

        [HttpPost]
        public async Task SaveAudioVerification([FromBody]SaveTranscriptionRequest request)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            await transcriptionService.SaveAudioVerificationAsync(request, user.Id);
        }

        [HttpPost]
        public async Task ReleaseAudio([FromBody]ReleaseFromProcessingRequest request)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            await transcriptionService.ReleaseTranscriptionAsync(request, user.Id);
        }

        [HttpGet]
        public async Task<GetTranscriptionResponse> GetAudioForTranscription()
            => await transcriptionService.GetForTranscriptionAsync();

        [HttpGet]
        public async Task<GetTranscriptionResponse> GetAudioForVerification()
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            return await transcriptionService.GetForVerificationAsync(user.Id);
        }

        [HttpGet("{id}")]
        public async Task<FileResult> GetAudioFile([FromRoute] long id)
        {
            var source = await labelingAudioService.GetAudioFileAsync(id);

            if (source == null)
                return null;

            source.Position = 0;
            return File(source, "audio/wav");
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("{instanceId}/{campaignId}/{importNumber}")]
        [DisableFormValueModelBinding]
        public async Task SaveAudio(long instanceId, long campaignId, long importNumber)
        {
            var boundary = MultipartRequestHelper.GetBoundary(Request.ContentType);
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType) || boundary == null)
                return;

            await transcriptionService.SaveAudioFileAsync(HttpContext.Request.Body, boundary, instanceId, campaignId, importNumber);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<long> GetNewImportNumber([FromBody] GetImportNumberRequest reqest)
            => await labelingAudioService.GetNewImportNumber(reqest);

        [HttpPost]
        public async Task<ImportStatisticsResponse[]> GetImportStatistics([FromBody] GetImportsReportRequest reqest)
            => await labelingStatisticsService.GetImportStatisticsAsync(reqest);

        [HttpPost]
        public async Task UpdateImport([FromBody] UpdateImportRequest request)
            => await transcriptionService.UpdateImportAsync(request);

        [HttpGet("{importId}")]
        public async Task<string[]> GetImportFileNames(long importId)
            => await transcriptionRepository.GetImportFileNamesAsync(importId);

        [HttpGet("{importId}/{type}")]
        public async Task ResetRecordsVerification(long importId, LabelingType type)
            => await importsRepository.ResetRecordsVerificationAsync(importId, type);

        [HttpGet]
        public async Task<CampaignInstancePairing[]> GetCampaignInstancePairing()
            => await transcriptionRepository.GetCampaignInstancePairingAsync();
    }
}