namespace Company.Gazoo.Controllers.WebApi
{
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = CustomPolicies.NextAmsPredictorOrMaster)]
    public class FCMomentsController : ControllerBase
    {
        private readonly UserManager<CompanyIdentityUser> userManager;
        private readonly IFCMomentsService fcMomentsService;
        private readonly ILabelingAudioService labelingAudioService;
        private readonly IFCMomentsRepository fcMomentsRepository;

        public FCMomentsController(IFCMomentsService fcMomentsService,
            UserManager<CompanyIdentityUser> userManager,
            IFCMomentsRepository fcMomentsRepository,
            ILabelingAudioService labelingAudioService)
        {
            this.userManager = userManager;
            this.fcMomentsService = fcMomentsService;
            this.labelingAudioService = labelingAudioService;
            this.fcMomentsRepository = fcMomentsRepository;
        }

        [HttpGet]
        public async Task<GetFCMomentResponse> GetAudioForProcessing()
            =>  await fcMomentsService.GetForProcessingAsync();

        [HttpGet]
        public async Task<GetFCMomentResponse> GetAudioForVerification()
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            return await fcMomentsService.GetForVerificationAsync(user.Id);
        }

        [HttpPost]
        public async Task ReleaseAudio([FromBody]ReleaseFromProcessingRequest request)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            await fcMomentsService.ReleaseMomentAsync(request, user.Id);
        }

        [HttpPost]
        public async Task SaveFCMomentResult([FromBody]SaveFCMomentResultRequest request)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            await fcMomentsService.SaveResultAsync(request, user.Id);
        }

        [HttpPost]
        public async Task SaveFCMomentVerificationResult([FromBody]SaveFCMomentResultRequest request)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            await fcMomentsService.SaveVerificationResultAsync(request, user.Id);
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

        [HttpPost]
        public async Task<SaveAudioResult> SaveAudioWithData()
        {
            var boundary = MultipartRequestHelper.GetBoundary(Request.ContentType);
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType) || boundary == null)
                return SaveAudioResult.UploadingError;

            return await fcMomentsService.SaveAudioWithDataAsync(HttpContext.Request.Body, boundary);
        }

        [HttpGet("{importId}")]
        public async Task<string[]> GetImportFileNames(long importId)
            => await fcMomentsRepository.GetImportFileNamesAsync(importId);
    }
}