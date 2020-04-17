using Company.Fred.Subscription.Communication.Configuration.Interfaces;

namespace Company.Examination.Audio.Integration.Communication.Configuration
{
    public interface IExaminationHostedServicesConfiguration
    {
        ICommunicationConfigurationSettings ExaminationActionsConfiguration { get; }
    }
}
