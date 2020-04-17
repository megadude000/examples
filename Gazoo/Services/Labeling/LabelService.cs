using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Company.Database.Utils.Transaction.Interfaces;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Enumerators.Labeling;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Company.Gazoo.Requests.Labeling;
using Company.Gazoo.Services.Labeling.Interfaces;

namespace Company.Gazoo.Services.Labeling
{
    internal class LabelService : ILabelService
    {
        private readonly IMapper mapper;
        private readonly ITransactionService transactionService;
        private readonly ILabelGroupsRepository labelGroupsRepository;
        private readonly ITranscriptionLabelsRepository labelsRepository;
        private readonly ITranscriptionSelectedLabelRepository transcriptionSelectedLabelRepository;

        public LabelService(IMapper mapper,
            ITransactionService transactionService,
            ILabelGroupsRepository labelGroupsRepository,
            ITranscriptionLabelsRepository labelsRepository,
            ITranscriptionSelectedLabelRepository transcriptionSelectedLabelRepository)
        {
            this.mapper = mapper;
            this.labelsRepository = labelsRepository;
            this.transactionService = transactionService;
            this.labelGroupsRepository = labelGroupsRepository;
            this.transcriptionSelectedLabelRepository = transcriptionSelectedLabelRepository;
        }

        public async Task SaveSelectedLabelsAsync(long[] selectedLabels, long? transcriptionId, long? recordingAudioChunkId)
        {
            var items = selectedLabels.Select(labelId => new TranscriptionSelectedLabel
            {
                LabelId = labelId,
                TranscriptionId = transcriptionId,
                ClientUtteranceId = recordingAudioChunkId
            }).ToArray();

            await transcriptionSelectedLabelRepository.AddRangeAsync(items);
        }

        public async Task<TranscriptionLabelModel[]> GetTranscriptionLabelsAsync(long groupId)
        {
            var transcriptionLabels = await labelsRepository.GetAllByGroupIdAsync(groupId);

            return mapper.Map<TranscriptionLabelModel[]>(transcriptionLabels);
        }

        public async Task AddLabelGroupAsync(AddLabelGroupRequest request)
        {
            var labelGroup = mapper.Map<LabelGroup>(request);

            await labelGroupsRepository.AddAsync(labelGroup);

            if (request.Type == LabelElementType.Checkbox)
                await labelsRepository.AddRangeAsync(GetChechboxLabels(labelGroup.Id));
        }

        public async Task UpdateLabelGroupAsync(UpdateLabelGroupRequest request)
        {
            await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                var oldElementType = await labelGroupsRepository.GetElementTypeAsync(request.Id);
                if (oldElementType != LabelElementType.Checkbox && request.Type == LabelElementType.Checkbox)
                {
                    await DeleteLabelsByGroupIdAsync(request.Id);
                    await labelsRepository.AddRangeAsync(GetChechboxLabels(request.Id));
                }

                await labelGroupsRepository.UpdateAsync(mapper.Map<LabelGroup>(request));
            });
        }

        public async Task SoftDeleteLabelGroupAsync(long labelGroupId)
        {
            var labelGroupToDelete = await labelGroupsRepository.GetByIdAsync(labelGroupId);
            labelGroupToDelete.IsDeleted = true;

            await labelGroupsRepository.UpdateAsync(labelGroupToDelete);
        }

        public async Task SoftDeleteLabelAsync(long labelId)
        {
            var labelToDelete = await labelsRepository.GetByIdAsync(labelId);
            labelToDelete.IsDeleted = true;

            await labelsRepository.UpdateAsync(labelToDelete);
        }

        public async Task DeleteLabelsByGroupIdAsync(long labelGroupId)
        {
            var labelsToDelete = await labelsRepository.GetAllByGroupIdAsync(labelGroupId);

            foreach (var label in labelsToDelete)
                label.IsDeleted = true;

            await labelsRepository.UpdateRangeAsync(labelsToDelete);
        }

        private TranscriptionLabel[] GetChechboxLabels(long labelGroupId)
            => new TranscriptionLabel[] {
                    new TranscriptionLabel { LabelGroupId = labelGroupId, Name = "true" },
                    new TranscriptionLabel { LabelGroupId = labelGroupId, Name = "false" }
                };
    }
}