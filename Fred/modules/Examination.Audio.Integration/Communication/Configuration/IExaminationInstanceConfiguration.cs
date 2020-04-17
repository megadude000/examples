using Company.Fred.Subscription.Communication.Configuration.Interfaces;
using Newtonsoft.Json;

namespace Company.Examination.Audio.Integration.Communication.Configuration
{
    public interface IExaminationInstanceConfiguration: IInstanceConfiguration
    {
        [JsonIgnore]
        long? InstanceId { get; set; }
    }
}
