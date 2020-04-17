using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Features.OwnedInstances;
using Company.Pebbles.Bedrock.Events;
using Company.Pebbles.Common.Interfaces;
using Company.Pebbles.Common.Utils;
using Company.Common.EventBus.Interfaces;
using log4net;

namespace Company.Pebbles.ViewModels
{
    internal sealed class ExaminationTemplateViewModel : BaseTemplateViewModel<ExaminationMainViewModel>, IDisposable
    {
        private readonly List<IDisposable> eventsSubscriptions = new List<IDisposable>();
        private readonly ILog logger;
        private readonly Owned<LoginViewModel> loginViewModel;

        public ExaminationTemplateViewModel(
           Owned<LoginViewModel> loginViewModel,
           ILog logger,
           IEventBus eventBus,
           IEnumerable<IExaminationLogonScopeProcess> logonScopeProcesses,
           Func<Owned<ExaminationMainViewModel>> examinationMainViewModel) : base(
               logger,
               logonScopeProcesses,
               examinationMainViewModel)
        {
            this.logger = logger;
            this.loginViewModel = loginViewModel;

            eventsSubscriptions.AddRange(new[]
            {
                eventBus.GetEvent<SuccesLogin>().Subscribe(
                    async x => await Handle(x),
                    ex => logger.Error("Error on observe SuccesLogin", ex))
            });
        }

        protected override Task BeginLogonSession()
        {
            ActivateView(loginViewModel.Value, PebblesInfo.InstanceName);
            return Task.CompletedTask;
        }

        private async Task Handle(SuccesLogin authorization)
        {
            logger.Info("Handle SuccesLogin");

            Loading = true;
            loginViewModel.Dispose();
            Loading = false;

            await base.BeginLogonSession();
        }

        public override void Dispose()
        {
            base.Dispose();

            loginViewModel.Dispose();

            eventsSubscriptions.ForEach(x => x.Dispose());
            eventsSubscriptions.Clear();
        }

    }
}
