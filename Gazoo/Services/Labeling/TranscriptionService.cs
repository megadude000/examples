using AutoMapper;
using Company.Brain.Communication.Contracts.DeepSpeech;
using Company.Database.Utils.Transaction.Interfaces;
using Company.Gazoo.Communication.Proxies.Interfaces;
using Company.Gazoo.Configuration;
using Company.Gazoo.Database.Entities.Enums;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Extensions;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Company.Gazoo.Repositories.Users.Interfaces;
using Company.Gazoo.Requests.Labeling;
using Company.Gazoo.Responses.Labeling;
using Company.Gazoo.Services.Labeling.Interfaces;
using Company.Gazoo.Utils;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling
{
    internal class TranscriptionService : ITranscriptionService
    {
        private readonly IMapper mapper;
        private readonly Random randomizer;
        private readonly IUsersRepository usersRepository;
        private readonly ITransactionService transactionService;
        private readonly IWorkingPeriodsService workingPeriodsService;
        private readonly IImportRepository importsRepository;
        private readonly ILabelingModuleConfiguration labelingModuleConfiguration;
        private readonly IAudioReopenSubscriptionService audioReopenSubscriptionService;
        private readonly IDeepSpeechControlProxy deepSpeechControlProxy;
        private readonly IAssignedLabelGroupsRepository assignedLabelGroupsRepository;
        private readonly ITranscriptionRepository transcriptionRepository;
        private readonly ILabelingAudioService labelingAudioService;
        private readonly ILabelService labelService;
        private readonly ITranscriptionLabelsRepository transcriptionLabelsRepository;
        private readonly ITranscriptionMetricsRepository transcriptionMetricsRepository;
        private readonly ITranscriptionSelectedLabelRepository transcriptionSelectedLabelRepository;

        public TranscriptionService(ITransactionService transactionService,
            IMapper mapper,
            IUsersRepository usersRepository,
            IImportRepository importsRepository,
            IWorkingPeriodsService workingPeriodsService,
            ITranscriptionRepository transcriptionRepository,
            ILabelingModuleConfiguration labelingModuleConfiguration,
            IAudioReopenSubscriptionService audioReopenSubscriptionService,
            IDeepSpeechControlProxy deepSpeechControlProxy,
            IAssignedLabelGroupsRepository assignedLabelGroupsRepository,
            ILabelingAudioService labelingAudioService,
            ILabelService labelService,
            ITranscriptionLabelsRepository transcriptionLabelsRepository,
            ITranscriptionMetricsRepository transcriptionMetricsRepository,
            ITranscriptionSelectedLabelRepository transcriptionSelectedLabelRepository)
        {
            this.mapper = mapper;
            this.transactionService = transactionService;
            this.usersRepository = usersRepository;
            this.workingPeriodsService = workingPeriodsService;
            this.importsRepository = importsRepository;
            this.deepSpeechControlProxy = deepSpeechControlProxy;
            this.labelingModuleConfiguration = labelingModuleConfiguration;
            this.audioReopenSubscriptionService = audioReopenSubscriptionService;
            this.transcriptionRepository = transcriptionRepository;
            this.assignedLabelGroupsRepository = assignedLabelGroupsRepository;
            this.labelingAudioService = labelingAudioService;
            this.labelService = labelService;
            this.transcriptionLabelsRepository = transcriptionLabelsRepository;
            this.transcriptionMetricsRepository = transcriptionMetricsRepository;
            this.transcriptionSelectedLabelRepository = transcriptionSelectedLabelRepository;

            randomizer = new Random();
        }

        public async Task UpdateImportAsync(UpdateImportRequest request)
        {
            await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                var import = await importsRepository.GetAsync(request.Id);

                import.Priority = request.Priority;
                import.Comment = request.Comment;

                await importsRepository.UpdateAsync(import);
            });
        }

        private async Task<TranscriptionLabels[]> GetAssignedLabels(long? importNumber)
        {
            var result = new List<TranscriptionLabels>();
            if (!importNumber.HasValue)
                return result.ToArray();

            var labelGroups = await assignedLabelGroupsRepository.GetAsync(importNumber.Value);

            foreach (var labelGroup in labelGroups)
            {
                var labels = await transcriptionLabelsRepository.GetAllByGroupIdAsync(labelGroup.Id);
                result.Add(new TranscriptionLabels
                {
                    LabelGroup = labelGroup,
                    Labels = mapper.Map<TranscriptionLabelModel[]>(labels)
                });
            }

            return result.ToArray();
        }

        public async Task SaveAudioTrancriptionAsync(SaveTranscriptionRequest request, long agentId)
        {
            UnsubscribeFromReopenAudio(request.Transcription.Id);

            await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                await SaveTranscription(request.Transcription, agentId);
                var filter = new WorkingPeriodFilter
                {
                    AgentId = agentId,
                    Type = LabelingType.Transcription,
                    Action = LabelingAction.Labeling
                };
                await workingPeriodsService.SaveWorkingPeriodAsync(filter, request.SpentTime);

                if (request.SelectedLabelIds.Any())
                    await labelService.SaveSelectedLabelsAsync(request.SelectedLabelIds, request.Transcription.Id, null);
            });
        }

        private async Task SaveTranscription(TranscriptionModel transcriptionModel, long agentId)
        {
            var transcription = await transcriptionRepository.GetAsync(transcriptionModel.Id);
            mapper.Map(transcriptionModel, transcription);
            transcription.AgentId = agentId;
            transcription.SaveTime = DateTime.UtcNow;
            transcription.InUse = false;
            CountErrorRates(ref transcription);

            await transcriptionRepository.UpdateAsync(transcription);
        }

        public async Task SaveAudioVerificationAsync(SaveTranscriptionRequest request, long agentId)
        {
            UnsubscribeFromReopenAudio(request.Transcription.Id);
            await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                await SaveVerifiedTranscription(request.Transcription, agentId);
                var filter = new WorkingPeriodFilter
                {
                    AgentId = agentId,
                    Type = LabelingType.Transcription,
                    Action = LabelingAction.Verification
                };
                await workingPeriodsService.SaveWorkingPeriodAsync(filter, request.SpentTime);
                await UpdateLabels(request.SelectedLabelIds, request.Transcription.Id);
            });
        }

        private async Task UpdateLabels(long[] selectedLabels, long transcriptionId)
        {
            var esixtingLabels = await transcriptionSelectedLabelRepository.GetAssignedAsync(transcriptionId);
            var existingSelectedLabelIds = esixtingLabels.Select(item => item.LabelId);
            var labelsToAdd = selectedLabels.Except(existingSelectedLabelIds);
            var labelsToDelete = existingSelectedLabelIds.Except(selectedLabels);

            if (labelsToAdd.Any())
            {
                foreach (var labelId in labelsToAdd)
                {
                    var selectedLabelObject = new TranscriptionSelectedLabel
                    {
                        LabelId = labelId,
                        TranscriptionId = transcriptionId
                    };
                    await transcriptionSelectedLabelRepository.AddAsync(selectedLabelObject);
                }
            }

            if (labelsToDelete.Any())
            {
                var labels = esixtingLabels.Where(item => labelsToDelete.Contains(item.LabelId)).ToArray();
                await transcriptionSelectedLabelRepository.RemoveRangeAsync(labels);
            }
        }

        private async Task SaveVerifiedTranscription(TranscriptionModel transcriptionModel, long agentId)
        {
            var transcription = await transcriptionRepository.GetAsync(transcriptionModel.Id);

            transcription.WordErrorRate = StringDistance.CountWordErrorRate(transcriptionModel.AgentTranscription, transcription.AgentTranscription);
            mapper.Map(transcriptionModel, transcription);

            transcription.VerifierId = agentId;
            transcription.VerificationTime = DateTime.UtcNow;
            transcription.InUse = false;

            CountErrorRates(ref transcription);

            await transcriptionRepository.UpdateAsync(transcription);
        }

        public async Task ReleaseTranscriptionAsync(ReleaseFromProcessingRequest request, long agentId)
        {
            await ReleaseTranscription(request.Id);
            var filter = new WorkingPeriodFilter
            {
                AgentId = agentId,
                Type = LabelingType.Transcription,
                Action = request.Action
            };
            await workingPeriodsService.SaveWorkingPeriodAsync(filter, request.SpentTime);
        }

        private async Task ReleaseTranscription(long id)
        {
            var transcriptionInfo = await transcriptionRepository.GetAsync(id);
            UnsubscribeFromReopenAudio(transcriptionInfo.Id);
            transcriptionInfo.InUse = false;
            await transcriptionRepository.UpdateAsync(transcriptionInfo);
        }

        public async Task ReleaseTranscriptionAsync(long id)
        {
            var transcriptionInfo = await transcriptionRepository.GetAsync(id);
            transcriptionInfo.InUse = false;
            await transcriptionRepository.UpdateAsync(transcriptionInfo);
        }

        public async Task ReleaseAllTranscriptionsAsync()
        {
            var inUseTranscriptions = await transcriptionRepository.GetInUseIds();

            if (!inUseTranscriptions.Any())
                return;

            foreach (var id in inUseTranscriptions)
                await ReleaseTranscriptionAsync(id);
        }

        public async Task<GetTranscriptionResponse> GetForTranscriptionAsync()
        {
            var transcriptionInfo = await transcriptionRepository.GetForTranscriptionAsync();
            if (transcriptionInfo == null)
                return null;

            transcriptionInfo.InUse = true;
            await transcriptionRepository.UpdateAsync(transcriptionInfo);
            var transcriptionLabels = await GetAssignedLabels(transcriptionInfo.ImportNumber);
            SubscribeToReopenAudio(transcriptionInfo.Id);
            return new GetTranscriptionResponse
            {
                Transcription = mapper.Map<TranscriptionModel>(transcriptionInfo),
                TranscriptionLabels = transcriptionLabels
            };
        }

        public async Task<GetTranscriptionResponse> GetForVerificationAsync(long agentId)
        {
            Transcription transcriptionInfo = null;

            if (labelingModuleConfiguration.ExpectedDailyVerificationsRatio > randomizer.NextDouble())
                transcriptionInfo = await transcriptionRepository.GetForCurrentDayVerificationAsync(agentId);

            if (transcriptionInfo == null)
                transcriptionInfo = await transcriptionRepository.GetForVerificationAsync(agentId);

            if (transcriptionInfo == null)
                return null;

            transcriptionInfo.InUse = true;
            await transcriptionRepository.UpdateAsync(transcriptionInfo);
            var transcriptionLabels = await GetAssignedLabels(transcriptionInfo.ImportNumber);
            var selectedLabels = await transcriptionSelectedLabelRepository.GetAssignedAsync(transcriptionInfo.Id);

            string authorFullName = null;
            if (await usersRepository.IsUserAssignedCustomClaimAsync(agentId, CustomClaims.ShowLabelerName))
            {
                var author = await usersRepository.GetUserAsync(transcriptionInfo.AgentId.Value);
                authorFullName = $"{author.Surname} {author.GivenName}";
            }

            SubscribeToReopenAudio(transcriptionInfo.Id);

            return new GetTranscriptionResponse
            {
                Transcription = mapper.Map<TranscriptionModel>(transcriptionInfo),
                TranscriptionLabels = transcriptionLabels,
                SelectedLabelIds = selectedLabels.Select(item => item.LabelId).ToArray(),
                AuthorFullName = authorFullName
            };
        }

        public async Task SaveAudioFileAsync(Stream stream, string boundary, long instanceId, long campaignId, long importNumber)
        {
            Transcription transcription = new Transcription();

            await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    transcription.AudioId = await labelingAudioService.SaveAudioFileAsync(memoryStream, boundary, instanceId, campaignId);
                    if (transcription.AudioId == 0)
                        return;

                    if (labelingModuleConfiguration.DeepSpeechControl.IsEnabled)
                    {
                        var result = await GetDeepSpeechTranscription(memoryStream, boundary);
                        transcription.DeepSpeechTranscription = result.Transcript;
                        transcription.MetricsId = await SaveTranscriptionMetrics(result.Score);
                    }

                    transcription.ImportNumber = importNumber == 0 ? null : (long?)importNumber;

                    await transcriptionRepository.AddAsync(transcription);
                    return;
                }
            });
        }

        private void CountErrorRates(ref Transcription transcription)
        {
            if (transcription.MetricsId.HasValue)
            {
                transcription.Metrics.WordErrorRate = StringDistance.CountWordErrorRate(transcription.AgentTranscription, transcription.DeepSpeechTranscription);
                transcription.Metrics.CharErrorRate = StringDistance.CountCharErrorRate(transcription.AgentTranscription, transcription.DeepSpeechTranscription);
            }
        }

        private async Task<long> SaveTranscriptionMetrics(double score)
        {
            var metric = new TranscriptionMetrics { DeepSpeechTranscriptionScore = score };

            await transcriptionMetricsRepository.AddAsync(metric);

            return metric.Id;
        }

        private async Task<TranscriptResult> GetDeepSpeechTranscription(MemoryStream memoryStream, string boundary)
        {
            MultipartSection section;
            memoryStream.Position = 0;
            var reader = new MultipartReader(boundary, memoryStream);

            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                if (section.HasHeaderFile())
                {
                    using (var wavFileStream = new MemoryStream())
                    {
                        section.Body.CopyTo(wavFileStream);
                        return await deepSpeechControlProxy.GetTranscriptionAsync(wavFileStream.ToArray());
                    }
                }
            }

            return new TranscriptResult();
        }

        private void SubscribeToReopenAudio(long id)
        {
            if (labelingModuleConfiguration.AudioReopen.IsEnabled)
                audioReopenSubscriptionService.Subscribe(id, LabelingType.Transcription);
        }

        private void UnsubscribeFromReopenAudio(long id)
        {
            if (labelingModuleConfiguration.AudioReopen.IsEnabled)
                audioReopenSubscriptionService.Unsubscribe(id, LabelingType.Transcription);
        }
    }
}
