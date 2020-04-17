using Autofac;
using Company.Pebbles.Examination.Services.Interfaces;
using Company.Pebbles.Examination.ViewModels;
using Company.Pebbles.Examination.ViewModels.Interfaces;

namespace Company.Pebbles.Examination.Modules
{
    public class ViewModelsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SummaryResultViewModel>();

            builder.RegisterType<SelectExaminationViewModel>();

            builder
                .RegisterType<ProgressViewModel>()
                .As<IProgressViewModel>()
                .InstancePerOwned<ISessionContext>();
        }
    }
}
