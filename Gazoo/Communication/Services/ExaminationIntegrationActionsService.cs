using Company.Gazoo.Communication.Contracts.AudioUpdateNotifications;
using Company.Gazoo.Communication.Contracts.SoundboardCommons;
using Company.Gazoo.Communication.Services.Interfaces;
using Company.Gazoo.Services.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Company.Gazoo.Communication.Services
{
    internal class ExaminationIntegrationActionsService : IExaminationIntegrationActionsService
    {
        private readonly ILogger<ExaminationIntegrationActionsService> logger;
        private readonly IExaminationIntegrationResponseService examinationIntegrationResponseService;

        public ExaminationIntegrationActionsService(ILogger<ExaminationIntegrationActionsService> logger,
            IExaminationIntegrationResponseService examinationIntegrationResponseService)
        {
            this.logger = logger;
            this.examinationIntegrationResponseService = examinationIntegrationResponseService;
        }

        public async Task<ResponseStatus> GetMissingFiles(MissingFilesRequest request, ServerCallContext context)
        {
            logger.LogInformation("Request for getting missing files has arrived. Start sending missing files.");
            try
            {
                await examinationIntegrationResponseService.SendMissingFiles(request);

                logger.LogInformation("Sending missing files finished successfully!");
                return new ResponseStatus() { NotificationStatus = NotificationStatus.Successful };
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, $"Error was occured while sending missing files.");
                return new ResponseStatus() { NotificationStatus = NotificationStatus.Failure };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error was occured while sending missing files.");
                return new ResponseStatus() { NotificationStatus = NotificationStatus.Failure };
            }
        }
    }
}