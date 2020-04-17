using Company.Pebbles.Examination.ViewModels.Interfaces;

namespace Company.Pebbles.Examination.Services.Interfaces
{
    public interface ISessionContext
    {
        IMediaService MediaService { get; }
        IExaminationTimer ExaminationTimer { get; }
        IExaminationService ExaminationService { get; }
        IAnswersService AnswersService { get; }
        IProgressViewModel ProgressViewModel { get; }
    }
}
