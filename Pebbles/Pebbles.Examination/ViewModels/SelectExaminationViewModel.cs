using Company.Pebbles.Configuration.Configuration;
using Company.Common.EventBus.Interfaces;
using Company.Pebbles.Examination.Models;
using Company.Pebbles.Examination.Properties;
using Company.Pebbles.Examination.Services.Interfaces;
using Company.Pebbles.Examination.ViewModels.Events;
using Company.Pebbles.WindowsCommonUtils.ViewModels;
using Company.Pebbles.WindowsCommonUtils.ViewModels.Interfaces;
using log4net;
using System;
using System.Windows;

namespace Company.Pebbles.Examination.ViewModels
{
    public class SelectExaminationViewModel : DialogBaseViewModel, IViewModel
    {
        private readonly ILog logger;
        private readonly IEventBus eventBus;
        private readonly IHttpRequestService examinationHttpRequestService;

        public bool Loading { get; set; } = true;
        public ExaminationModel[] Examinations { get; set; }

        public SelectExaminationViewModel(ILog logger,
            ISettings settings,
            IEventBus eventBus,
            IHttpRequestService examinationHttpRequestService) : base(settings)
        {
            this.logger = logger;
            this.eventBus = eventBus;
            this.examinationHttpRequestService = examinationHttpRequestService;

            DisplayName = Resources.SelectExaminationHeader;
        }

        public void ExaminationDoubleClick(ExaminationModel examination)
        {
            eventBus.Publish(new SelectedExamination(examination.Id));
            TryClose();
        }

        protected override async void OnViewLoaded(object view)
        {
            try
            {
                Loading = true;

                Examinations = await examinationHttpRequestService.GetExaminationsAsync();
            }
            catch (Exception exception)
            {
                HandleException(Resources.ExaminationsLoadingError, Resources.Error, exception);
                TryClose();
            }
            finally
            {
                Loading = false;
            }

            base.OnViewLoaded(view);
        }

        private void HandleException(string message, string caption, Exception exception)
        {
            logger.Error(exception);
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
