using Company.Pebbles.Configuration.Configuration;
using Company.Common.EventBus.Interfaces;
using Company.Pebbles.Examination.Models;
using Company.Pebbles.Examination.Properties;
using Company.Pebbles.Examination.ViewModels.Events;
using Company.Pebbles.WindowsCommonUtils.ViewModels;
using Company.Pebbles.WindowsCommonUtils.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using ILog = log4net.ILog;

namespace Company.Pebbles.Examination.ViewModels
{
    public class SummaryResultViewModel : DialogBaseViewModel, IViewModel
    {
        private readonly ILog logger;
        private readonly IDisposable summarySubscription;

        public ExaminationSummaryModel ExaminationSummaryModel { get; set; }

        public SummaryResultViewModel(ILog logger,
            ISettings settings,
            IEventBus eventBus) : base(settings)
        {
            this.logger = logger;

            DisplayName = Resources.SummaryResult;

            summarySubscription = eventBus.GetEvent<ShowSummary>().Subscribe(
                    x => Handle(x),
                    ex => logger.Error("Error on observe ShowSummary", ex));
        }

        public void Close()
        {
            TryClose();
        }

        public void Handle(ShowSummary message)
        {
            ExaminationSummaryModel = message.Summary;
        }

        protected override void OnDeactivate(bool close)
        {
            summarySubscription.Dispose();

            base.OnDeactivate(close);
        }
    }
}
