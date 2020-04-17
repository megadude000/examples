using AutoMapper;
using Company.Database.Utils.Transaction.Interfaces;
using Company.Gazoo.Configuration;
using Company.Gazoo.Database.Entities.Enums;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Extensions;
using Company.Gazoo.Models.AudioFile;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Company.Gazoo.Repositories.Users.Interfaces;
using Company.Gazoo.Requests.Labeling;
using Company.Gazoo.Responses.Labeling;
using Company.Gazoo.Services.Labeling.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling
{
    internal class FCMomentsService : IFCMomentsService
    {
        private readonly IMapper mapper;
        private readonly IUsersRepository usersRepository;
        private readonly ITransactionService transactionService;
        private readonly IWorkingPeriodsService workingPeriodsService;
        private readonly ILabelingModuleConfiguration labelingModuleConfiguration;
        private readonly IFCMomentsRepository fcMomentsRepository;
        private readonly IAudioReopenSubscriptionService audioReopenSubscriptionService;
        private readonly ILabelingAudioService labelingAudioService;
        private readonly IFCMomentAMLogRepository fcMomentAMLogRepository;

        public FCMomentsService(IMapper mapper,
            ITransactionService transactionService,
            IUsersRepository usersRepository,
            ILabelingModuleConfiguration labelingModuleConfiguration,
            IWorkingPeriodsService workingPeriodsService,
            IFCMomentsRepository fcMomentsRepository,
            IAudioReopenSubscriptionService audioReopenSubscriptionService,
            ILabelingAudioService labelingAudioService,
            IFCMomentAMLogRepository fcMomentAMLogRepository)
        {
            this.mapper = mapper;
            this.usersRepository = usersRepository;
            this.transactionService = transactionService;
            this.workingPeriodsService = workingPeriodsService;
            this.labelingModuleConfiguration = labelingModuleConfiguration;
            this.audioReopenSubscriptionService = audioReopenSubscriptionService;
            this.fcMomentsRepository = fcMomentsRepository;
            this.labelingAudioService = labelingAudioService;
            this.fcMomentAMLogRepository = fcMomentAMLogRepository;
        }

        public async Task SaveResultAsync(SaveFCMomentResultRequest request, long autorId)
        {
            var fcMomentInfo = await fcMomentsRepository.GetAsync(request.Id);

            fcMomentInfo.InUse = false;
            fcMomentInfo.AuthorId = autorId;
            fcMomentInfo.SaveTime = DateTime.UtcNow;
            fcMomentInfo.BestAnswerTime = TimeSpan.FromSeconds(request.BestAnswerTime);
            fcMomentInfo.SelectedAms = request.Result;
            fcMomentInfo.IsPerfect = request.IsPerfect;

            UnsubscribeFromReopenAudio(fcMomentInfo.Id);
            await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                await fcMomentsRepository.UpdateAsync(fcMomentInfo);
                var filter = new WorkingPeriodFilter
                {
                    AgentId = autorId,
                    Type = LabelingType.FullConversationMoments,
                    Action = LabelingAction.Labeling
                };
                await workingPeriodsService.SaveWorkingPeriodAsync(filter, request.SpentTime);
            });
        }

        public async Task SaveVerificationResultAsync(SaveFCMomentResultRequest request, long autorId)
        {
            var fcMomentInfo = await fcMomentsRepository.GetAsync(request.Id);

            fcMomentInfo.InUse = false;
            fcMomentInfo.VerifierId = autorId;
            fcMomentInfo.VerificationTime = DateTime.UtcNow;
            fcMomentInfo.BestAnswerTime = TimeSpan.FromSeconds(request.BestAnswerTime);
            fcMomentInfo.SelectedAms = request.Result;
            fcMomentInfo.IsPerfect = request.IsPerfect;

            UnsubscribeFromReopenAudio(fcMomentInfo.Id);
            await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                await fcMomentsRepository.UpdateAsync(fcMomentInfo);
                var filter = new WorkingPeriodFilter
                {
                    AgentId = autorId,
                    Type = LabelingType.FullConversationMoments,
                    Action = LabelingAction.Verification
                };
                await workingPeriodsService.SaveWorkingPeriodAsync(filter, request.SpentTime);
            });
        }

        public async Task ReleaseMomentAsync(ReleaseFromProcessingRequest request, long agentId)
        {
            await ReleaseMoment(request.Id);
            var filter = new WorkingPeriodFilter
            {
                AgentId = agentId,
                Type = LabelingType.FullConversationMoments,
                Action = request.Action
            };
            await workingPeriodsService.SaveWorkingPeriodAsync(filter, request.SpentTime);
        }

        private async Task ReleaseMoment(long id)
        {
            var fcMomentInfo = await fcMomentsRepository.GetAsync(id);
            fcMomentInfo.InUse = false;
            await fcMomentsRepository.UpdateAsync(fcMomentInfo);
            UnsubscribeFromReopenAudio(fcMomentInfo.Id);
        }

        public async Task ReleaseMomentAsync(long id)
        {
            var momentInfo = await fcMomentsRepository.GetByAudioIdAsync(id);
            momentInfo.InUse = false;
            await fcMomentsRepository.UpdateAsync(momentInfo);
        }

        public async Task ReleaseAllMomentsAsync()
        {
            var inUseMoments = await fcMomentsRepository.GetInUseIds();

            if (!inUseMoments.Any())
                return;

            foreach (var id in inUseMoments)
                await ReleaseMoment(id);
        }

        public async Task<GetFCMomentResponse> GetForProcessingAsync()
        {
            var momentInfo = await fcMomentsRepository.GetForProcessingAsync();
            if (momentInfo == null)
                return null;

            return await PrepareMomentForResponse(momentInfo);
        }

        public async Task<GetFCMomentResponse> GetForVerificationAsync(long userId)
        {
            var momentInfo = await fcMomentsRepository.GetForVerificationAsync(userId);
            if (momentInfo == null)
                return null;

            var response = await PrepareMomentForResponse(momentInfo);

            if(await usersRepository.IsUserAssignedCustomClaimAsync(userId, CustomClaims.ShowLabelerName))
            {
                var author = await usersRepository.GetUserAsync(momentInfo.AuthorId.Value);
                response.AuthorFullName = $"{author.Surname} {author.GivenName}";
            }

            return response;
        }

        private async Task<GetFCMomentResponse> PrepareMomentForResponse(FCMoment momentInfo)
        {
            momentInfo.InUse = true;
            await fcMomentsRepository.UpdateAsync(momentInfo);
            var amLogs = await fcMomentAMLogRepository.GetRangeByMomentIdAsync(momentInfo.Id);
            SubscribeToReopenAudio(momentInfo.Id);
            return new GetFCMomentResponse
            {
                NextAmsPredictionModel = mapper.Map<FCMomentModel>(momentInfo),
                AudioMessageLogs = mapper.Map<FCMomentAMLogModel[]>(amLogs)
            };
        }

        public async Task<SaveAudioResult> SaveAudioWithDataAsync(Stream stream, string boundary)
        {
            return await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                MultipartSection section;
                var reader = new MultipartReader(boundary, stream);
                var momentData = new FCMomentDataModel[] { };
                var audioFileInfo = new FCMomentAudioFileInfo();

                while ((section = await reader.ReadNextSectionAsync()) != null)
                {
                    if (section.HasHeaderInfo("audioFileInfo"))
                    {
                        audioFileInfo = await DeserializeSection<FCMomentAudioFileInfo>(section);
                    }

                    if (section.HasHeaderInfo("momentData"))
                    {
                        momentData = await DeserializeSection<FCMomentDataModel[]>(section);
                    }

                    if (section.HasHeaderFile() && audioFileInfo != null && momentData.Any())
                    {
                        var audioId = await labelingAudioService.SaveAudioFileAsync(section.Body, audioFileInfo);
                        if (audioId == 0)
                        {
                            return SaveAudioResult.UploadingError;
                        }

                        var moments = momentData.Select(item => new FCMoment
                        {
                            InUse = false,
                            AudioId = audioId,
                            PossibleAms = item.PossibleAMs,
                            ImportNumber = audioFileInfo.ImportNumber,
                            SourceInputName = item.SourceInputName
                        }).ToArray();

                        await fcMomentsRepository.AddRangeAsync(moments);

                        foreach (var item in moments)
                        {
                            var amLogs = momentData.Where(model => model.SourceInputName == item.SourceInputName).Single();
                            await SaveAudioMessageLog(amLogs.AudioMessageLog, item.Id);
                        }
                    }
                }

                return SaveAudioResult.Ok;
            });
        }

        private async Task SaveAudioMessageLog(FCMomentAMLogModel[] amLogModels, long momentId)
        {
            var mappedAMlogs = mapper.Map<FCMomentAMLog[]>(amLogModels);

            foreach (var log in mappedAMlogs)
            {
                log.MomentId = momentId;
                await fcMomentAMLogRepository.AddAsync(log);
            }
        }

        private async Task<TResponse> DeserializeSection<TResponse>(MultipartSection section)
        {
            using (var streamReader = new StreamReader(section.Body))
            {
                return JsonConvert.DeserializeObject<TResponse>(await streamReader.ReadToEndAsync());
            }
        }

        private void SubscribeToReopenAudio(long id)
        {
            if (labelingModuleConfiguration.AudioReopen.IsEnabled)
                audioReopenSubscriptionService.Subscribe(id, LabelingType.FullConversationMoments);
        }

        private void UnsubscribeFromReopenAudio(long id)
        {
            if (labelingModuleConfiguration.AudioReopen.IsEnabled)
                audioReopenSubscriptionService.Unsubscribe(id, LabelingType.FullConversationMoments);
        }
    }
}
