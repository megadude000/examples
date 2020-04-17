using FluentFTP;
using Company.Examination.Audio.Integration.Configuration.Interfaces;
using Company.Examination.Audio.Integration.Services.Interfaces;
using Company.Fred.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Company.Examination.Audio.Integration.Services
{
    internal class RemoteFileService : IRemoteFileService
    {
        private readonly ILogger<RemoteFileService> logger;
        private readonly IExaminationAudioSynchronizationConfiguration examinationAudioSynchronizationConfiguration;

        public RemoteFileService(ILogger<RemoteFileService> logger,
            IExaminationAudioSynchronizationConfiguration examinationAudioSynchronizationConfiguration)
        {
            this.logger = logger;
            this.examinationAudioSynchronizationConfiguration = examinationAudioSynchronizationConfiguration;
        }

        public async Task DownloadFileAsync(IFtpConfiguration centralFtpConfiguration, string rempteFilePath)
        {
            logger.LogInformation($"Downloading {rempteFilePath} from central Gazoo ftp");

            using (var ftpClient = InitializeFtpClient(centralFtpConfiguration))
            {
                var localFilePath = Path.Combine(examinationAudioSynchronizationConfiguration.AudioFilesWorkingDirectory, Path.GetFileName(rempteFilePath));

                try
                {
                    await ftpClient.DownloadFileAsync(localFilePath, rempteFilePath);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error on downloading file from remote ftp server. File name {Path.GetFileName(rempteFilePath)}");

                    if (File.Exists(localFilePath))
                        File.Delete(localFilePath);
                }
            }
        }

        private IFtpClient InitializeFtpClient(IFtpConfiguration ftpConfiguration)
        {
            try
            {
                var ftpClient = new FtpClient
                {
                    Host = ftpConfiguration.Host,
                    Credentials = new NetworkCredential(ftpConfiguration.Login, ftpConfiguration.Password),
                    Port = ftpConfiguration.Port
                };

                ftpClient.Connect();

                return ftpClient;
            }
            catch (SocketException exception)
            {
                logger.LogError($"Error during trying to initialize connection to FTP. Exception: {exception}");
                throw;
            }
            catch (TimeoutException exception)
            {
                logger.LogError($"Time for initialize connection to FTP is over. Exception: {exception}");
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError($"Error was occured while trying initialize connection to FTP. Exception: {exception}");
                throw;
            }
        }
    }
}
