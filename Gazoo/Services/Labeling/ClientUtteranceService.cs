using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Company.Gazoo.Services.Labeling.Interfaces;
using Company.Gazoo.Requests.Labeling;

namespace Company.Gazoo.Services.Labeling
{
    internal class ClientUtteranceService : IClientUtteranceService
    {
        private readonly IClientUtteranceRepository clientUtteranceRepository;
        private readonly IAudioMessageLogUpdateRepository audioMessageLogUpdateRepository;
        private readonly ITranscriptionSelectedLabelRepository transcriptionSelectedLabelRepository;
        private readonly IMapper mapper;

        public ClientUtteranceService(IClientUtteranceRepository clientUtteranceRepository,
            IAudioMessageLogUpdateRepository audioMessageLogUpdateRepository,
            ITranscriptionSelectedLabelRepository transcriptionSelectedLabelRepository,
            IMapper mapper)
        {
            this.clientUtteranceRepository = clientUtteranceRepository;
            this.audioMessageLogUpdateRepository = audioMessageLogUpdateRepository;
            this.transcriptionSelectedLabelRepository = transcriptionSelectedLabelRepository;
            this.mapper = mapper;
        }

        public async Task UpdateRecordingLabelingAsync(LabelingChankCreatorRequest request, long agentId)
        {
            await UpdateClientUtterances(request.CallId, agentId, request.ClientUtterances);
            await UpdateAudioMessageLogs(request.CallId, agentId, request.AudioMessageLogsUpdate);
        }

        private async Task UpdateClientUtterances(long callId, long agentId, ClientUtteranceRequest[] newClientUtterancesRequest)
        {
            var oldClientUtterances = await clientUtteranceRepository.GetByCallIdAsync(callId);

            if (oldClientUtterances.Any())
                await clientUtteranceRepository.RemoveRangeAsync(oldClientUtterances);

            if (newClientUtterancesRequest.Any())
            {
                var newClientUtterances = newClientUtterancesRequest
                    .Select(request => new ClientUtterance
                    {
                        CallId = callId,
                        AuthorId = agentId,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        SelectedLabel = request.SelectedLabels.Select(labelId => new TranscriptionSelectedLabel
                        {
                            LabelId = labelId
                        }).ToArray()
                    }).ToArray();

                await clientUtteranceRepository.AddRangeAsync(newClientUtterances);
            }
        }

        private async Task UpdateAudioMessageLogs(long callId, long agentId, AudioMessageLogUpdateRequest[] newLogs)
        {
            if (await audioMessageLogUpdateRepository.ExistAsync(callId))
            {
                var audioMessageLogsToUpdate = await audioMessageLogUpdateRepository.GetByCallIdAsync(callId);
                audioMessageLogsToUpdate
                    .Join(newLogs, messageLog => messageLog.Id, messageRequest => messageRequest.Id, (log, request) => mapper.Map(request, log))
                    .ToArray();
                await audioMessageLogUpdateRepository.UpdateRangeAsync(audioMessageLogsToUpdate);
            }
            else
            {
                var audioMessageLogsToUpdate = newLogs
                    .Select(rowMessageRequest => mapper.Map<AudioMessageLogUpdateRequest, AudioMessageLogUpdate>(rowMessageRequest,
                        options => options.AfterMap((source, dest) =>
                        {
                            dest.CallId = callId;
                            dest.AuthorId = agentId;
                        }))).ToArray();
                await audioMessageLogUpdateRepository.AddRangeAsync(audioMessageLogsToUpdate);
            }
        }
    }
}
