using Autofac;
using Company.Examination.Audio.Integration.Services;
using Company.Examination.Audio.Integration.Services.Interfaces;

namespace Company.Examination.Audio.Integration.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<RemoteFileService>()
                .As<IRemoteFileService>();

            builder
                .RegisterType<ExaminationAudioProcessingService>()
                .As<IExaminationAudioProcessingService>();

            builder
                .RegisterType<RemoteConfigurationService>()
                .As<IRemoteConfigurationService>();
        }
    }
}
