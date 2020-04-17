using Autofac;
using Company.Pebbles.Examination.AppStartup;
using Company.Pebbles.Common.Interfaces;

namespace Company.Pebbles.Examination.Modules
{
    internal class StartupModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<GetExaminationRemoteSettings>()
                .As<IExaminationLogonScopeProcess>();
        }
    }
}
