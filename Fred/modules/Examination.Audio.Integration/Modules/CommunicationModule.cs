using Autofac;
using Company.Examination.Audio.Integration.Communication;
using Company.Examination.Audio.Integration.Communication.Services;
using Company.Examination.Audio.Integration.Communication.Services.Interfaces;
using Company.Examination.Audio.Integration.Communication.Services.Proxies;
using Company.Fred.Core.Interfaces;
using static Company.Gazoo.Communication.Contracts.AudioUpdateNotifications.AudioUpdateNotificationService;

namespace Company.Examination.Audio.Integration.Modules
{
    public class CommunicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ExaminationIntegrationActionsProxy>()
                .As<IExaminationIntegrationActionsProxy>();

            builder
                .RegisterType<ExaminationIntegrationService>()
                .As<IExaminationIntegrationActionsService>();

            builder
                .RegisterType<ServicesHoster>()
                .As<IServicesHoster>();

            builder
                .RegisterType<ExaminationIntegrationServiceProxy>()
                .As<AudioUpdateNotificationServiceBase>();

            builder
                .RegisterType<AudioUpdateNotificationServiceClient>()
                .AsSelf();
        }
    }
}
