using Company.Examination.Audio.Integration.Communication.Configuration;
using Company.Examination.Audio.Integration.Configuration.Interfaces;
using Company.Examination.Audio.Integration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Company.Examination.Audio.Integration.Controllers.WebApi
{
    [Route("api/[controller]/[action]")]
    public class ExaminationIntegrationController : Controller
    {
        private string centralGazooConnectionString { get => $"http://{centralGazooAddressConfiguration.Host}:{centralGazooAddressConfiguration.Port}"; }

        private readonly ILogger<ExaminationIntegrationController> logger;
        private readonly ICentralGazooAddressConfiguration centralGazooAddressConfiguration;
        private readonly IExaminationInstanceConfiguration examinationInstanceConfiguration;
        private readonly IExaminationAudioSynchronizationConfiguration examinationAudioSynchronizationConfiguration;

        public ExaminationIntegrationController(ICentralGazooAddressConfiguration centralGazooAddressConfiguration,
            ILogger<ExaminationIntegrationController> logger,
            IExaminationInstanceConfiguration examinationInstanceConfiguration,
            IExaminationAudioSynchronizationConfiguration examinationAudioSynchronizationConfiguration)
        {
            this.logger = logger;
            this.centralGazooAddressConfiguration = centralGazooAddressConfiguration;
            this.examinationInstanceConfiguration = examinationInstanceConfiguration;
            this.examinationAudioSynchronizationConfiguration = examinationAudioSynchronizationConfiguration;
        }

        [HttpGet]
        public ExaminationRemoteSettings GetExaminationRemoteSettings()
        {
            if (!examinationInstanceConfiguration.InstanceId.HasValue)
            {
                logger.LogWarning("Examination remote settings can't be returned with default instance value.");
                return null;
            }

            return new ExaminationRemoteSettings
            {
                FTPExaminationDirectory = examinationAudioSynchronizationConfiguration.AudioFilesWorkingDirectory,
                CentralInstanceConnectionString = centralGazooConnectionString,
                InstanceId = examinationInstanceConfiguration.InstanceId.Value
            };
        }
    }
}
