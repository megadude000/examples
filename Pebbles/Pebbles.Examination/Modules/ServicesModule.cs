using Autofac;
using Company.Pebbles.Examination.Services;
using Company.Pebbles.Examination.Services.Interfaces;

namespace Company.Barney.Modules
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ExaminationService>()
                .As<IExaminationService>()
                .InstancePerOwned<ISessionContext>();

            builder
                .RegisterType<ExaminationTimer>()
                .As<IExaminationTimer>()
                .InstancePerOwned<ISessionContext>();

            builder
                .RegisterType<AnswersService>()
                .As<IAnswersService>()
                .InstancePerOwned<ISessionContext>();

            builder
                .RegisterType<IterationsService>()
                .As<IIterationsService>();

            builder
                .RegisterType<ReportService>()
                .As<IReportService>();

            builder
                .RegisterType<MediaService>()
                .As<IMediaService>()
                .InstancePerOwned<ISessionContext>();

            builder
                .RegisterType<GivenAnswerService>()
                .As<IGivenAnswerService>();

            builder
                .RegisterType<HttpRequestService>()
                .As<IHttpRequestService>();

            builder
                .RegisterType<SessionContext>()
                .As<ISessionContext>();
        }
    }
}
