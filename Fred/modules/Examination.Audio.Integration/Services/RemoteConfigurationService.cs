using Company.Examination.Audio.Integration.Communication.Configuration;
using Company.Examination.Audio.Integration.Configuration.Interfaces;
using Company.Examination.Audio.Integration.Helpers;
using Company.Examination.Audio.Integration.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Company.Examination.Audio.Integration.Services
{
    internal class RemoteConfigurationService : IRemoteConfigurationService
    {
        private readonly ICentralGazooAddressConfiguration centralGazooAddressConfiguration;
        private IExaminationInstanceConfiguration instanceConfiguration;

        public RemoteConfigurationService(IExaminationInstanceConfiguration instanceConfiguration,
            ICentralGazooAddressConfiguration centralGazooAddressConfiguration)
        {
            this.instanceConfiguration = instanceConfiguration;
            this.centralGazooAddressConfiguration = centralGazooAddressConfiguration;
        }

        public async Task GetInstanceId()
        {
            var url = $"http://{centralGazooAddressConfiguration.Host}:{centralGazooAddressConfiguration.Port}{CentralInstanceRequestConstants.GetInstanceIdByName}/{instanceConfiguration.Name}";

            using (HttpClient client = new HttpClient { BaseAddress = new Uri(url) })
            {
                var response = await client.GetAsync(client.BaseAddress.AbsolutePath);
                var responseString = await response.Content.ReadAsStringAsync();
                instanceConfiguration.InstanceId = JsonConvert.DeserializeObject<long>(responseString);
            }
        }
    }
}
