using Autofac;
using Autofac.Features.OwnedInstances;
using Company.Pebbles.Configuration.Configuration;
using Company.Common.EventBus.Interfaces;
using Company.Pebbles.Core.Services.Interfaces;
using Company.Pebbles.Examination.Enumerations;
using Company.Pebbles.Examination.Models;
using Company.Pebbles.Examination.Properties;
using Company.Pebbles.Examination.Services.Interfaces;
using Company.Pebbles.Examination.ViewModels;
using Company.Pebbles.Examination.ViewModels.Events;
using Company.Pebbles.Examination.ViewModels.Interfaces;
using Company.Pebbles.ViewModels.Events;
using Company.Pebbles.WindowsCommonUtils.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Company.Pebbles.Enumerations.ColumnsDivision;
using ILog = log4net.ILog;

namespace Company.Pebbles.ViewModels
{
    internal class ExaminationMainViewModel : BaseMainViewModel
    {
        private readonly EventLoopScheduler backgroundSheduler = new EventLoopScheduler(ts => new Thread(ts) { IsBackground = true });
        private readonly Func<Owned<ISessionContext>> sessionContextFactory;
        private readonly IEventBus eventBus;
        private readonly List<IDisposable> eventsSubscriptions = new List<IDisposable>();

        private string searchScriptsString;
        private string searchObjectionsString;
        private string searchQuickAnswersString;

        private Owned<ISessionContext> sessionContextOwned;

        public IProgressViewModel ProgressViewModel { get; private set; }

        public ExaminationAnswerModel[] ExaminationScripts { get; set; }
        public ExaminationAnswerModel[] ExaminationObjections { get; set; }
        public ExaminationAnswerModel[] ExaminationQuickAnswers { get; set; }

        public bool IsColumnsScriptsDivided { get; set; }
        public bool IsColumnsObjectonsDivided { get; set; }
        public bool IsColumnsQuickAnswersDivided { get; set; }

        public int ColumnAmountScripts => IsColumnsScriptsDivided ? (int)TwoColumns : (int)OneColumn;
        public int ColumnAmountObjections => IsColumnsObjectonsDivided ? (int)TwoColumns : (int)OneColumn;
        public int ColumnAmountQuickAnswers => IsColumnsQuickAnswersDivided ? (int)TwoColumns : (int)OneColumn;

        public int CurrentExaminationProgress { get; set; }
        public bool IsLoading { get; set; } = false;
        public bool IsListViewActive { get; set; } = false;
        public bool IsButtonNextActive { get; set; } = false;
        public bool IsExaminationRunning { get; set; } = false;
        public Visibility ExaminationVisibility { get => !IsButtonNextActive && IsExaminationRunning ? Visibility.Visible : Visibility.Hidden; }

        public AnswerStatus CurrentAnswerStatus { get; set; }
        public string SearchObjectionsString
        {
            get { return searchObjectionsString; }
            set
            {
                searchObjectionsString = value;
                ExaminationObjections = GetSearchedAnswers(sessionContextOwned?.Value.AnswersService.Objections, value);
            }
        }

        public string SearchScriptsString
        {
            get => searchScriptsString;
            set
            {
                searchScriptsString = value;
                ExaminationScripts = GetSearchedAnswers(sessionContextOwned?.Value.AnswersService.Scripts, value);
            }
        }

        public string SearchQuickAnswersString
        {
            get { return searchQuickAnswersString; }
            set
            {
                searchQuickAnswersString = value;
                ExaminationQuickAnswers = GetSearchedAnswers(sessionContextOwned?.Value.AnswersService.QuickAnswers, value);
            }
        }

        public ExaminationMainViewModel(ISaveableSettings settings,
            IWindowService windowService,
            ILog logger,
            IScenarioStorageService scenarioStorageService,
            IEventBus eventBus,
            IRemoteGeneralSettings generalSettings,
            Func<Owned<ISessionContext>> sessionContextFactory)
            : base(settings, windowService, logger, scenarioStorageService, generalSettings)
        {
            this.eventBus = eventBus;
            this.sessionContextFactory = sessionContextFactory;
        }

        public override void OnVolumeValueChanged()
        {
            base.OnVolumeValueChanged();
            sessionContextOwned?.Value?.MediaService?.ChangeVolume(normalizedVolume);
        }

        protected override void OnInitialize()
        {
            VolumeValue = RemoteGeneralSettings.Audio.MaxVolume;
            SubscribeEvents();
        }

        public void SetDivisionOfColumnsScripts()
        {
            IsColumnsScriptsDivided = !IsColumnsScriptsDivided;
        }

        public void SetDivisionOfColumnsObjections()
        {
            IsColumnsObjectonsDivided = !IsColumnsObjectonsDivided;
        }

        public void SetDivisionOfColumnsQuickAnswers()
        {
            IsColumnsQuickAnswersDivided = !IsColumnsQuickAnswersDivided;
        }

        public void SwitchExaminationPause()
        {
            if (IsExaminationRunning)
                StopExamination(false);
            else
                SelectExamination();
        }

        private void SelectExamination()
        {
            windowService.ShowDialog<SelectExaminationViewModel>();
        }

        public void LogoutAgent()
        {
            logger.Info("Logout requested by agent");

            eventBus.Publish(new LogoutAgent());
            UnsubscribeEvents();
        }

        public async Task StopExamination(bool isEmergency)
        {
            if (sessionContextOwned.Value.ExaminationService == null)
                return;
            try
            {
                IsLoading = true;
                sessionContextOwned.Value.ExaminationService.StopAudioIfPlayed();
                var summary = sessionContextOwned.Value.ExaminationService.GetExaminationSummary();
                await sessionContextOwned.Value.ExaminationService.StopExamination();
                SetViewControlsToDefault();
                if (!isEmergency)
                {
                    if (summary.AnsweredTestCasesCount != 0 || summary.TimeUps != 0)
                        windowService.ShowDialog<SummaryResultViewModel, ShowSummary>(new ShowSummary(summary));
                }
                IsLoading = false;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                logger.Info(Resources.StopBeforeExaminationHasStart, exception);
                ExaminationStoppedHandler();
            }
            catch (WebException exception)
            {
                HandleException(Resources.RemoteServerExceptionMessage, Resources.Error, exception);
                ExaminationStoppedHandler();
            }
            catch (Exception exception)
            {
                HandleException(Resources.ErrorInApplicationViewLogs, Resources.Error, exception);
                ExaminationStoppedHandler();
            }
        }

        public void NextTestCase()
        {
            try
            {
                IsButtonNextActive = false;
                SetViewControlsWhilePlayBack();
                sessionContextOwned.Value.ExaminationService.MoveToNextTextCase(normalizedVolume);
            }
            catch (FileNotFoundException ex)
            {
                HandleException($"{Resources.AudioFileIsAbsent} {Resources.ContactSystemAdministrator}", Resources.Error, ex);
                StopExamination(true);
            }
            catch (FormatException ex)
            {
                HandleException($"{Resources.IncorrectFileFormat} {Resources.ContactSystemAdministrator}", Resources.Error, ex);
                StopExamination(true);
            }
            catch (Exception ex)
            {
                HandleException(Resources.ErrorInApplicationViewLogs, Resources.Error, ex);
                StopExamination(true);
            }
        }

        public void AnswerDoubleClick(ExaminationAnswerModel answer)
        {
            if (!sessionContextOwned.Value.ExaminationTimer.TimerActive)
                return;

            if (!sessionContextOwned.Value.ExaminationService.IsNextAnswerNeeded(answer))
            {
                CurrentAnswerStatus = sessionContextOwned.Value.ExaminationService.StopTestCase(answer);
                SetViewControlsAfterAnswerGiven();
            }
        }

        public void ClearScriptsSearch()
        {
            SearchScriptsString = string.Empty;
        }

        public void ClearObjectionsSearch()
        {
            SearchObjectionsString = string.Empty;
        }

        public void ClearQuickAnswersSearch()
        {
            SearchQuickAnswersString = string.Empty;
        }

        private void SetViewControlsWhilePlayBack()
        {
            CurrentProgress = 100;
            CurrentAnswerStatus = AnswerStatus.None;
            IsListViewActive = true;
        }

        private void SetViewControlsAfterAnswerGiven()
        {
            IsListViewActive = false;
        }

        private void SetViewControlsToDefault()
        {
            CurrentProgress = 0;
            CurrentAnswerStatus = AnswerStatus.None;
            IsListViewActive = false;
        }

        private async Task StartExamination(long examinationId)
        {
            SubscribeExaminationEvents();
            try
            {
                await sessionContextOwned.Value.ExaminationService.StartExamination(examinationId);
                SetAnswers();
                ChangeExamState();
                IsButtonNextActive = true;
            }
            catch (FileNotFoundException ex)
            {
                HandleException($"{Resources.AudioFileIsAbsent} {Resources.ContactSystemAdministrator}", Resources.Error, ex);
                UnsubscribeExaminationEvents();
            }
            catch (Exception ex)
            {
                HandleException(Resources.ErrorInApplicationViewLogs, Resources.Error, ex);
                UnsubscribeExaminationEvents();
            }
        }

        private void StartExamSession()
        {
            sessionContextOwned = sessionContextFactory();

            ProgressViewModel = sessionContextOwned.Value.ProgressViewModel;
        }

        private void SubscribeExaminationEvents()
        {
            StartExamSession();

            sessionContextOwned.Value.ExaminationService.ExaminationStopped += ExaminationStoppedHandler;
            sessionContextOwned.Value.ExaminationTimer.ProgressChanged += UpdateProgressBarHandler;
            sessionContextOwned.Value.ExaminationTimer.TimeUp += TimeUpHandler;
        }

        private void UnsubscribeExaminationEvents()
        {
            sessionContextOwned.Value.ExaminationService.ExaminationStopped -= ExaminationStoppedHandler;
            sessionContextOwned.Value.ExaminationTimer.ProgressChanged -= UpdateProgressBarHandler;
            sessionContextOwned.Value.ExaminationTimer.TimeUp -= TimeUpHandler;
            ProgressViewModel = null;

            sessionContextOwned?.Dispose();
        }

        private void SetAnswers()
        {
            ExaminationQuickAnswers = sessionContextOwned.Value.AnswersService.QuickAnswers.ToArray();
            ExaminationScripts = sessionContextOwned.Value.AnswersService.Scripts.ToArray();
            ExaminationObjections = sessionContextOwned.Value.AnswersService.Objections.ToArray();
        }

        private void ClearAnswers()
        {
            SearchObjectionsString = string.Empty;
            SearchQuickAnswersString = string.Empty;
            SearchScriptsString = string.Empty;
            ExaminationQuickAnswers = null;
            ExaminationScripts = null;
            ExaminationObjections = null;
            sessionContextOwned.Value.AnswersService.ClearAnswers();
        }

        private void ExaminationStoppedHandler()
        {
            CurrentProgress = 0;
            CurrentAnswerStatus = AnswerStatus.None;
            IsListViewActive = false;
            IsButtonNextActive = false;
            ClearAnswers();
            ChangeExamState();
            UnsubscribeExaminationEvents();
            IsLoading = false;
        }

        private void UpdateProgressBarHandler(int progress)
        {
            if (IsExaminationRunning)
                CurrentProgress = progress;
        }

        private void TimeUpHandler()
        {
            SetViewControlsAfterAnswerGiven();
            CurrentProgress = 0;
            CurrentAnswerStatus = AnswerStatus.TimeUp;
        }

        private void ChangeExamState()
        {
            IsExaminationRunning = !IsExaminationRunning;
            IsButtonNextActive = IsExaminationRunning;
        }

        private void HandleException(string message, string caption, Exception exception)
        {
            logger.Error(exception);
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            IsLoading = false;
        }

        private void SubscribeEvents()
        {
            eventsSubscriptions.AddRange(new[] {
                eventBus.GetEvent<SelectedExamination>().ObserveOn(backgroundSheduler).Subscribe(
                    async x => await Handle(x),
                    ex => logger.Error("Error on observe SelectedExamination", ex))}
            );
        }

        private void UnsubscribeEvents()
        {
            eventsSubscriptions.ForEach(x => x.Dispose());
            eventsSubscriptions.Clear();
            backgroundSheduler.Dispose();
        }

        private async Task Handle(SelectedExamination message)
        {
            logger.Info("Handle SelectedExamination");

            IsLoading = true;
            await StartExamination(message.ExaminationId);
            IsLoading = false;
        }

        protected T[] GetSearchedAnswers<T>(IEnumerable<T> answers, string searchString) where T : ExaminationAnswerModel
        {
            if (!string.IsNullOrEmpty(searchString))
                return answers?.Where(answer => answer.Value.Trim().ToLower()
                        .Contains(searchString.Trim().ToLower())).ToArray();

            return answers?.ToArray();
        }

        protected override void OnDeactivate(bool close)
        {
            UnsubscribeEvents();

            if (IsExaminationRunning)
            {
                Task.Run(() => StopExamination(true)).Wait();
            }

            sessionContextOwned?.Dispose();

            base.OnDeactivate(close);
        }
    }
}
