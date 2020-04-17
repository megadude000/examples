using Company.Pebbles.Configuration.Configuration;
using Company.Pebbles.Configuration.Providers.Interfaces;
using Company.Pebbles.Core.Services.Interfaces;
using Company.Pebbles.Examination.Enumerations;
using Company.Pebbles.Examination.Models;
using Company.Pebbles.Examination.Properties;
using Company.Pebbles.Examination.Services.Interfaces;
using Company.Pebbles.Examination.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IMediaService = Company.Pebbles.Examination.Services.Interfaces.IMediaService;

namespace Company.Pebbles.Examination.Services
{
    internal class ExaminationService : IExaminationService
    {
        private readonly IDateTimeService dateTimeService;
        private readonly IWorkingDirectoryProvider workingDirectoryProvider;
        private readonly IExaminationInformation examinationInformation;
        private readonly IMediaService mediaService;
        private readonly IExaminationTimer examinationTimer;
        private readonly IReportService reportService;
        private readonly IAnswersService answersService;
        private readonly IIterationsService iterationsService;
        private readonly IProgressViewModel progressViewModel;
        private readonly IGivenAnswerService givenAnswerService;
        private readonly IHttpRequestService httpRequestService;
        private readonly ILocalAudioMessageService localAudioMessageService;
        private readonly IExaminationRemoteSettings settings;
        private TimeSpan totalReactionTime;
        private IDictionary<string, ExaminationQuestionModel> examinationQuestions;
        private bool IsAudioPlaying { get { return mediaService.GetPlaybackState() == NAudio.Wave.PlaybackState.Playing; } }

        public event Action ExaminationStopped;
        public int TestCasesCount { get { return examinationQuestions.Count(); } }
        public bool IsNextTestCaseExist { get { return iterationsService.IsNextExist; } }

        public ExaminationService(IDateTimeService dateTimeService,
            IWorkingDirectoryProvider workingDirectoryProvider,
            IExaminationInformation examinationInformation,
            IMediaService mediaService,
            IExaminationTimer examinationTimer,
            IReportService reportService,
            IAnswersService answersService,
            IProgressViewModel progressViewModel,
            IIterationsService iterationsService,
            IGivenAnswerService givenAnswerService,
            IHttpRequestService httpRequestService,
            ILocalAudioMessageService localAudioMessageService,
            IExaminationRemoteSettings settings)
        {
            this.dateTimeService = dateTimeService;
            this.workingDirectoryProvider = workingDirectoryProvider;
            this.examinationInformation = examinationInformation;
            this.examinationTimer = examinationTimer;
            this.mediaService = mediaService;
            this.reportService = reportService;
            this.answersService = answersService;
            this.iterationsService = iterationsService;
            this.progressViewModel = progressViewModel;
            this.givenAnswerService = givenAnswerService;
            this.httpRequestService = httpRequestService;
            this.localAudioMessageService = localAudioMessageService;
            this.settings = settings;
            this.examinationTimer.TimeUp += TimeUpHandler;
        }

        public async Task StartExamination(long examinationId)
        {
            if (IsAudioPlaying)
                return;

            await LoadExaminationData(examinationId);

            progressViewModel.SetTotalTestCasesCount(TestCasesCount);
            progressViewModel.SetPressNextToStartLabel();
        }

        public bool IsNextAnswerNeeded(ExaminationAnswerModel selectedAnswer)
        {
            return givenAnswerService.IsNextAnswerNeeded(selectedAnswer, examinationTimer.GetElapsedTime());
        }

        public AnswerStatus StopTestCase(ExaminationAnswerModel selectedAnswer)
        {
            examinationTimer.Stop();
            StopAudioPlaying();

            var answerStatus = givenAnswerService.IsAnswerCorrect() ? AnswerStatus.Correct : AnswerStatus.Incorrect;
            SaveAnswer(givenAnswerService.GivenAnswer, givenAnswerService.ElapsedTime, answerStatus);

            return answerStatus;
        }

        public void MoveToNextTextCase(float volume)
        {
            if (IsAudioPlaying)
                return;

            PlayNextAudio(volume);
        }

        public async Task StopExamination()
        {
            progressViewModel.SetExamPassLabel();

            if (examinationQuestions != null && !string.IsNullOrEmpty(iterationsService.Current))
            {
                SaveRemainingCasesToReport();
                await reportService.SaveReportsToServer();
            }

            ExaminationStopped?.Invoke();
        }

        public ExaminationSummaryModel GetExaminationSummary()
        {
            var summary = reportService.GetExaminationSummary();
            summary.TotalTestCasesCount = TestCasesCount;

            return summary;
        }

        public void StopAudioIfPlayed()
        {
            examinationTimer.Stop();
            if (IsAudioPlaying)
            {
                StopAudioPlaying();
            }
        }

        private void StopAudioPlaying()
        {
            if (IsAudioPlaying)
            {
                mediaService.StopAudioPlaying();
            }
        }

        private void SaveRemainingCasesToReport()
        {
            if (!reportService.IsReportExist(iterationsService.Current))
                reportService.AddReport(iterationsService.Current,
                    TimeSpan.Zero,
                    new ExaminationAnswerModel(),
                    AnswerStatus.Stopped,
                    examinationQuestions[iterationsService.Current].QuestionId);

            while (IsNextTestCaseExist)
            {
                var iteration = iterationsService.NextIteration();
                reportService.AddReport(iteration,
                    TimeSpan.Zero,
                    new ExaminationAnswerModel(),
                    AnswerStatus.Stopped,
                    examinationQuestions[iterationsService.Current].QuestionId);
            }
        }

        private void PlayNextAudio(float volume = default(float))
        {
            var nextIteration = iterationsService.NextIteration();
            givenAnswerService.SetCurrentQuestionIntoService(examinationQuestions[iterationsService.Current]);
            mediaService.StartAudioPlaying(nextIteration, workingDirectoryProvider.ExaminationAudioFolder, volume);
            totalReactionTime = TimeSpan.FromSeconds(examinationQuestions[iterationsService.Current].MaxReactionTime) + mediaService.CurrentAudioDuration;
            StartCountdownTimer(iterationsService.Current);
            progressViewModel.SetCurrentProgress();
        }

        private void StartCountdownTimer(string filePath)
        {
            examinationTimer.Start(totalReactionTime);
        }

        private void SaveAnswer(ExaminationAnswerModel answer, TimeSpan spentTime, AnswerStatus answerStatus)
        {
            reportService.AddReport(
                iterationsService.Current,
                TimeSpan.FromMilliseconds(GetReactionTime(spentTime, answerStatus)),
                answer,
                answerStatus,
                examinationQuestions[iterationsService.Current].QuestionId);

            progressViewModel.SetGivenAnswerStatusLabel(answerStatus, IsNextTestCaseExist);

            if (IsNextTestCaseExist)
                PlayNextAudio();
        }

        private double GetReactionTime(TimeSpan spentTime, AnswerStatus answerStatus)
        {
            double reactionTime = 0;
            var expectedReactionTime = TimeSpan.FromSeconds(examinationQuestions[iterationsService.Current].DefaultReactionTime);
            var additionalRactionTime = examinationQuestions[iterationsService.Current].MaxReactionTime;
            switch (answerStatus)
            {
                case (AnswerStatus.Correct):
                case (AnswerStatus.Incorrect):
                    {
                        var difference = TimeSpan.FromSeconds(examinationQuestions[iterationsService.Current].DefaultReactionTime) - spentTime;
                        reactionTime = Math.Abs(difference.TotalMilliseconds);
                        break;
                    }
                case (AnswerStatus.TimeUp):
                    {
                        var difference = totalReactionTime - expectedReactionTime;
                        var maxTimeAsTimeSpan = TimeSpan.FromSeconds(examinationQuestions[iterationsService.Current].MaxReactionTime);
                        reactionTime = difference <= maxTimeAsTimeSpan ? TimeSpan.FromSeconds(additionalRactionTime).TotalMilliseconds
                                                                       : Math.Abs(difference.TotalMilliseconds);
                        break;
                    }
                default:
                    break;
            }

            return reactionTime;
        }

        private void TimeUpHandler()
        {
            StopAudioPlaying();
            SaveAnswer(givenAnswerService.GivenAnswer, TimeSpan.Zero, AnswerStatus.TimeUp);
        }

        private async Task LoadExaminationData(long examinationId)
        {
            var questions = await httpRequestService.GetExaminationDataAsync(examinationId);
            CreatDictionaryFromExaminationQuestions(questions);
            await SetExaminationAnswers(examinationId);
            await PrepareAudioFilesAndIterations();

            examinationInformation.State.CurrentExaminationId = examinationId;
            examinationInformation.State.StartTime = dateTimeService.GetCurrentServerDateTime();
        }


        private async Task SetExaminationAnswers(long examinationId)
        {
            if (!examinationQuestions.Any())
                throw new ArgumentException(Resources.ExaminationQuestionsIsAbsent);
            await answersService.SetAnswers(examinationId);
        }

        private async Task PrepareAudioFilesAndIterations()
        {
            var audioNames = examinationQuestions.Select(question => question.Value.AudioFileName).ToArray();
            if (!audioNames.Any())
                throw new FileNotFoundException(Resources.AudioFilesNotFound);

            await localAudioMessageService.PullChangedAudiosAsync(audioNames, settings.FTPExaminationDirectory, workingDirectoryProvider.ExaminationAudioFolder);
            iterationsService.SetIterations(audioNames, examinationQuestions.First().Value.IsRandom);
        }

        private void CreatDictionaryFromExaminationQuestions(ExaminationQuestionModel[] questions)
        {
            examinationQuestions = questions
                                    .Select(question => new { Key = question.AudioFileName, Value = question })
                                    .ToDictionary(item => item.Key, item => item.Value);
        }
    }
}
