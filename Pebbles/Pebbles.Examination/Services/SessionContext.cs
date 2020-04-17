using Company.Pebbles.Examination.Services.Interfaces;
using Company.Pebbles.Examination.ViewModels.Interfaces;

namespace Company.Pebbles.Examination.Services
{
    internal class SessionContext : ISessionContext
    {
        public IMediaService MediaService { get; private set; }
        public IExaminationTimer ExaminationTimer { get; private set; }
        public IExaminationService ExaminationService { get; private set; }
        public IAnswersService AnswersService { get; private set; }
        public IProgressViewModel ProgressViewModel { get; private set; }

        public SessionContext(IExaminationTimer examinationTimer,
            IMediaService mediaService,
            IExaminationService examinationService,
            IAnswersService answersService,
            IProgressViewModel progressViewModel)
        {
            MediaService = mediaService;
            ExaminationTimer = examinationTimer;
            ExaminationService = examinationService;
            AnswersService = answersService;
            ProgressViewModel = progressViewModel;
        }
    }
}
