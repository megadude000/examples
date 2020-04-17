using Autofac;
using Company.Examination.Audio.Integration.Communication.Configuration;
using Company.Examination.Audio.Integration.Configuration.Interfaces;
using Company.Fred.Services.Utils;
using Company.Fred.Subscription.Communication.Configuration.Interfaces;
using Microsoft.DotNet.PlatformAbstractions;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Company.Examination.Audio.Integration.Modules
{
    public class ConfigurationModule : Module
    {
        private IntegrationConfiguration integrationConfiguration;

        protected override void Load(ContainerBuilder builder)
        {
            GetIntegrationSettings();

            builder.Register(CreateExaminationAudioSynchronizationConfiguration).SingleInstance();
            builder.Register(CreateCentralGazooAddressConfiguration).SingleInstance();
            builder.Register(CreateDependentServicesConfiguration).SingleInstance();
            builder.Register(CreateHostedServicesConfiguration).SingleInstance();
            builder.Register(CreateInstanceConfiguration).SingleInstance();
        }

        private ICentralGazooAddressConfiguration CreateCentralGazooAddressConfiguration(IComponentContext context)
        {
            return integrationConfiguration.CentralGazooAddressConfiguration;
        }

        private IExaminationAudioSynchronizationConfiguration CreateExaminationAudioSynchronizationConfiguration(IComponentContext context)
        {
            return integrationConfiguration.ExaminationAudioSynchronizationConfiguration;
        }

        private IExaminationHostedServicesConfiguration CreateHostedServicesConfiguration(IComponentContext context)
        {
            return integrationConfiguration.HostedServicesConfiguration;
        }

        private IExaminationDependentServicesConfiguration CreateDependentServicesConfiguration(IComponentContext context)
        {
            return integrationConfiguration.DependentServicesConfiguration;
        }

        private IExaminationInstanceConfiguration CreateInstanceConfiguration(IComponentContext context)
        {
            return integrationConfiguration.InstanceConfiguration;
        }

        private void GetIntegrationSettings()
        {
            var applicationBasePath = AppContext.BaseDirectory;
            var path = Path.Combine(applicationBasePath, "Config", "Company.Examination.Audio.Integration.json");
            var fileContent = File.ReadAllText(path);
            integrationConfiguration =  JsonConvert.DeserializeObject<IntegrationConfiguration>(fileContent);
        }

        private class IntegrationConfiguration
        {
            [JsonProperty("CentralGazooAddress")]
            [JsonConverter(typeof(TypeSerializer<CentralGazooAddressConfiguration>))]
            public ICentralGazooAddressConfiguration CentralGazooAddressConfiguration { get; set; }

            [JsonProperty("ExaminationAudioSynchronizationConfiguration")]
            [JsonConverter(typeof(TypeSerializer<ExaminationAudioSynchronizationConfiguration>))]
            public IExaminationAudioSynchronizationConfiguration ExaminationAudioSynchronizationConfiguration { get; set; }

            [JsonProperty("HostedServices")]
            [JsonConverter(typeof(TypeSerializer<HostedServicesConfiguration>))]
            public IExaminationHostedServicesConfiguration HostedServicesConfiguration { get; set; }

            [JsonProperty("DependentServices")]
            [JsonConverter(typeof(TypeSerializer<DependentServicesConfiguration>))]
            public IExaminationDependentServicesConfiguration DependentServicesConfiguration { get; set; }

            [JsonProperty("Instance")]
            [JsonConverter(typeof(TypeSerializer<InstanceConfiguration>))]
            public IExaminationInstanceConfiguration InstanceConfiguration { get; set; }
        }

        private class HostedServicesConfiguration : IExaminationHostedServicesConfiguration
        {
            [JsonProperty("AudioUpdates")]
            [JsonConverter(typeof(TypeSerializer<CommunicationConfigurationSettings>))]
            public ICommunicationConfigurationSettings ExaminationActionsConfiguration { get; set; }
        }

        private class DependentServicesConfiguration : IExaminationDependentServicesConfiguration
        {
            [JsonProperty("ExaminationIntegration")]
            [JsonConverter(typeof(TypeSerializer<CommunicationConfigurationSettings>))]
            public ICommunicationConfigurationSettings ExaminationIntegrationConfiguration { get; set; }
        }

        private class CentralGazooAddressConfiguration : ICentralGazooAddressConfiguration
        {
            public string Host { get; set; }
            public string Port { get; set; }
        }

        private class ExaminationAudioSynchronizationConfiguration : IExaminationAudioSynchronizationConfiguration
        {
            public string AudioFilesWorkingDirectory { get; set; }
        }

        private class InstanceConfiguration : IExaminationInstanceConfiguration
        {
            public string Name { get; set; }

            public bool IsSyncEnabled { get; set; }

            public long? InstanceId { get; set; }
        }

        private class CommunicationConfigurationSettings : ICommunicationConfigurationSettings
        {
            public string Ip { get; set; }
            public int Port { get; set; }
        }
    }
}
