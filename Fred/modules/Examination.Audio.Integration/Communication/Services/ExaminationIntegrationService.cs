using Company.Examination.Audio.Integration.Communication.Services.Interfaces;
using Company.Examination.Audio.Integration.Configuration.Interfaces;
using Company.Examination.Audio.Integration.Services.Interfaces;
using Company.Fred.Core.Interfaces;
using Company.Fred.Core.Models;
using Company.Fred.Services.Interfaces;
using Company.Gazoo.Communication.Contracts.AudioUpdateNotifications;
using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Company.Examination.Audio.Integration.Communication.Services
{
    internal class ExaminationIntegrationService : IExaminationIntegrationActionsService
    {
        private readonly IRemoteFileService remoteFileService;
        private readonly ILocalFileService localFileService;
        private readonly ILogger<ExaminationIntegrationService> logger;
        private readonly IExaminationAudioSynchronizationConfiguration examinationAudioSynchronizationConfiguration;

        public ExaminationIntegrationService(IRemoteFileService remoteFileService,
            ILocalFileService localFileService,
            ILogger<ExaminationIntegrationService> logger,
            IExaminationAudioSynchronizationConfiguration examinationAudioSynchronizationConfiguration)
        {
            this.logger = logger;
            this.localFileService = localFileService;
            this.remoteFileService = remoteFileService;
            this.examinationAudioSynchronizationConfiguration = examinationAudioSynchronizationConfiguration;
        }

        public async Task<ResponseStatus> AudioUpdatesAvailable(NotificationRequest request, ServerCallContext context)
        {
            logger.LogInformation($"New examination audio update available! Action: {request.Action}. Name: {Path.GetFileNameWithoutExtension(request.FileName)}. Start updating.");
            try
            {
                switch (request.Action)
                {
                    case NotificationAction.Add:
                        {
                            var centralGazooFtpConfiguration = new FtpConfigurationModel
                            {
                                Host = request.FtpIp,
                                Port = request.FtpPort,
                                Login = request.FtpLogin,
                                Password = request.FtpPassword
                            };

                            await remoteFileService.DownloadFileAsync(centralGazooFtpConfiguration, request.FileName);
                            break;
                        }

                    case NotificationAction.Delete:
                        localFileService.DeleteFile(CreateFilePath(request.FileName));
                        break;

                    case NotificationAction.Update:
                        localFileService.RenameFile(CreateFilePath(request.OldFileName), CreateFilePath(request.FileName));
                        break;

                    default:
                        break;
                }
                logger.LogInformation($"Updating examination audio: {Path.GetFileNameWithoutExtension(request.FileName)} finished successfully!");
                return new ResponseStatus() { NotificationStatus = NotificationStatus.Successful };
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, $"Error was occured while updating examination audio file: {Path.GetFileNameWithoutExtension(request.FileName)}.");
                return new ResponseStatus() { NotificationStatus = NotificationStatus.Failure };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error was occured while updating examination audio file: {Path.GetFileNameWithoutExtension(request.FileName)}.");
                return new ResponseStatus() { NotificationStatus = NotificationStatus.Failure };
            }
        }

        private string CreateFilePath(string fileName)
        {
            return Path.Combine(examinationAudioSynchronizationConfiguration.AudioFilesWorkingDirectory, $"{fileName}.wav");
        }
    }
}
