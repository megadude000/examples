using Company.Gazoo.Configuration;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Database.Entities.Enums;
using Company.Gazoo.Extensions;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Company.Gazoo.Requests.Labeling;
using Company.Gazoo.Services.Labeling.Interfaces;
using Company.Gazoo.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Labeling
{
    internal class LabelingAudioService : ILabelingAudioService
    {
        private readonly IImportRepository importRepository;
        private readonly IFilesConfiguration filesConfiguration;
        private readonly IWavFileDecodeService wavFileDecodeService;
        private readonly IAssignedLabelGroupsRepository assignedLabelGroupsRepository;
        private readonly ILabelingAudioRepository labelingAudioRepository;

        public LabelingAudioService(IFilesConfiguration filesConfiguration,
            IWavFileDecodeService wavFileDecodeService,
            IImportRepository importRepository,
            IAssignedLabelGroupsRepository assignedLabelGroupsRepository,
            ILabelingAudioRepository labelingAudioRepository)
        {
            this.importRepository = importRepository;
            this.filesConfiguration = filesConfiguration;
            this.wavFileDecodeService = wavFileDecodeService;
            this.assignedLabelGroupsRepository = assignedLabelGroupsRepository;
            this.labelingAudioRepository = labelingAudioRepository;
        }

        public async Task<long> GetNewImportNumber(GetImportNumberRequest request)
        {
            var newImportNumber = new ImportNumber
            {
                Comment = request.Comment,
                Priority = request.Priority,
                Type = (LabelingType)request.Type
            };

            await importRepository.AddAsync(newImportNumber);

            if (request.AssignedLabels == null)
                return newImportNumber.Id;

            var assignedImports = request.AssignedLabels.Select(item => new AssignedLabelGroups
            {
                ImportNumber = newImportNumber.Id,
                LabelGroupId = item
            }).ToArray();

            await assignedLabelGroupsRepository.AddRangeAsync(assignedImports);

            return newImportNumber.Id;
        }

        public async Task<Stream> GetAudioFileAsync(long id)
        {
            var audioFileInfo = await labelingAudioRepository.GetByIdAsync(id);

            return audioFileInfo != null ? new FileStream(Path.Combine(filesConfiguration.ConversationTranscription, audioFileInfo.FilePath), FileMode.Open, FileAccess.Read) : null;
        }

        public async Task<long> SaveAudioFileAsync(MemoryStream stream, string boundary, long instanceId, long campaignId)
        {
            MultipartSection section;
            var audioFile = new LabelingAudio();

            stream.Position = 0;
            var reader = new MultipartReader(boundary, stream);

            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                if (section.HasHeaderInfo("file"))
                {
                    ContentDisposition contentDisposition = new ContentDisposition(section.ContentDisposition);
                    audioFile.FileName = contentDisposition.FileName.Remove(contentDisposition.FileName.Length - 4);

                    if (await CheckIfFileExist(audioFile.FileName, instanceId, campaignId))
                    {
                        return 0;
                    }
                }
                if (section.HasHeaderFile() && !string.IsNullOrEmpty(audioFile.FileName))
                {
                    using (var wavFileStream = CreateMemoryStream(section.Body))
                    {
                        audioFile.AudioDuration = GetAudioDuration(wavFileStream);

                        if (audioFile.AudioDuration.TotalSeconds == 0)
                            return 0;

                        await SaveAudioFileToLocalFolder(wavFileStream, audioFile.FileName, instanceId, campaignId);
                    }
                }
            }
            audioFile.CampaignId = campaignId;
            audioFile.InstanceId = instanceId;
            audioFile.FilePath = GenerateFilePath(audioFile.FileName, instanceId, campaignId);
            await labelingAudioRepository.AddAsync(audioFile);
            return audioFile.Id;
        }

        public async Task<long> SaveAudioFileAsync(Stream fileStream, FCMomentAudioFileInfo fileInfo)
        {
            var audioFile = new LabelingAudio
            {
                FileName = Guid.NewGuid().ToString("N")
            };

            if (await CheckIfFileExist(audioFile.FileName))
                return 0;

            using (var wavFileStream = CreateMemoryStream(fileStream))
            {
                audioFile.AudioDuration = GetAudioDuration(wavFileStream);

                if (audioFile.AudioDuration.TotalSeconds == 0)
                    return 0;

                await SaveAudioFileToLocalFolder(wavFileStream, audioFile.FileName, fileInfo.ImportNumber);
            }


            audioFile.CallId = fileInfo.CallId;
            audioFile.FilePath = GenerateFilePath(audioFile.FileName, fileInfo.ImportNumber);
            await labelingAudioRepository.AddAsync(audioFile);
            return audioFile.Id;
        }

        private string GenerateFilePath(string fileName, long instanceId, long campaignId)
        {
            return $"{instanceId}_{campaignId}/{fileName}{filesConfiguration.AudioFileExtention}";
        }

        private string GenerateFilePath(string fileName, long importId)
        {
            return $"{importId}/{fileName}{filesConfiguration.AudioFileExtention}";
        }

        private async Task<bool> CheckIfFileExist(string request, long instanceId = 0, long campaignId = 0)
        {
            return await labelingAudioRepository.IsExistAsync(request, instanceId, campaignId);
        }

        private MemoryStream CreateMemoryStream(Stream source)
        {
            var memoryStream = new MemoryStream();
            source.CopyTo(memoryStream);
            return memoryStream;
        }

        private TimeSpan GetAudioDuration(MemoryStream wavFileStream)
        {
            return TimeSpan.FromSeconds(wavFileDecodeService.GetDurationInSeconds(wavFileStream));
        }

        private async Task SaveAudioFileToLocalFolder(Stream wavFile, string fileName, long instanceId, long campaignId)
        {
            var filePath = Path.Combine(filesConfiguration.ConversationTranscription, GenerateFilePath(fileName, instanceId, campaignId));
            Directory.CreateDirectory(Path.Combine(filesConfiguration.ConversationTranscription, $"{instanceId}_{campaignId}"));

            wavFile.Position = 0;
            using (var targetStream = File.Create(filePath))
            {
                await wavFile.CopyToAsync(targetStream);
                wavFile.Dispose();
            }
        }

        private async Task SaveAudioFileToLocalFolder(Stream wavFile, string fileName, long importId)
        {
            var filePath = Path.Combine(filesConfiguration.ConversationTranscription, GenerateFilePath(fileName, importId));
            Directory.CreateDirectory(Path.Combine(filesConfiguration.ConversationTranscription, $"{importId}"));

            wavFile.Position = 0;
            using (var targetStream = File.Create(filePath))
            {
                await wavFile.CopyToAsync(targetStream);
                wavFile.Dispose();
            }
        }
    }
}
