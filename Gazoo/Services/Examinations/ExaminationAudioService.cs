using Company.Database.Utils.Transaction.Interfaces;
using Company.Gazoo.Configuration;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Extensions;
using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Company.Gazoo.Requests;
using Company.Gazoo.Services.Examinations.Interfaces;
using Company.Gazoo.Services.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations
{
    internal class ExaminationAudioService : IExaminationAudioService
    {
        private readonly IFilesConfiguration filesConfiguration;
        private readonly ITransactionService transactionService;
        private readonly IExaminationQuestionsRepository questionsRepository;
        private readonly IExaminationQuestionAudioFilesRepository questionAudioFilesRepository;

        public ExaminationAudioService(IFilesConfiguration filesConfiguration,
            ITransactionService transactionService,
            IExaminationQuestionsRepository questionsRepository,
            IExaminationQuestionAudioFilesRepository questionAudioFilesRepository)
        {
            this.filesConfiguration = filesConfiguration;
            this.transactionService = transactionService;
            this.questionsRepository = questionsRepository;
            this.questionAudioFilesRepository = questionAudioFilesRepository;
        }

        public async Task<Stream> GetAsync(long id)
        {
            var audioFileInfo = await questionAudioFilesRepository.GetAsync(id);
            
            return audioFileInfo != null ? new FileStream(audioFileInfo.Path, FileMode.Open, FileAccess.Read) : null;
        }

        public async Task<DeleteExaminationAudioResult> DeleteAsync(long[] filesIds)
        {
            var response = DeleteExaminationAudioResult.Error;
            response = await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                if (!await questionsRepository.AudioIsUsedAsync(filesIds))
                {
                    await questionAudioFilesRepository.SoftDeleteRangeAsync(filesIds);

                    return DeleteExaminationAudioResult.Ok;
                }
                return DeleteExaminationAudioResult.Using;
            });

            if (response == DeleteExaminationAudioResult.Ok)
            {
                var fileNames = await questionAudioFilesRepository.GetNamesRangeAsync(filesIds);
                BackgroundJob.Enqueue<IExaminationIntegrationResponseService>(service => service.NotifyLocalInstancesAboutDeleteAudioFile(fileNames));
            }

            return response;
        }

        public async Task<bool> UpdateAsync(UpdateExaminationAudioFileInfoRequest request)
        {
            string oldFileName = string.Empty;

            var response = await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                var fileInfo = await questionAudioFilesRepository.GetAsync((long)request.Id);

                if (fileInfo.IsDeleted)
                    return false;

                oldFileName = fileInfo.Name;

                if (await CheckAudioInfoForUpdate(fileInfo, request))
                {
                    if (fileInfo.Name != request.FileName)
                    {
                        var newFilePath = CreateFilePath(request.FileName);
                        File.Delete(newFilePath);
                        File.Move(fileInfo.Path, newFilePath);

                        fileInfo.Path = newFilePath;
                    }

                    fileInfo.Comment = request.Comment;
                    fileInfo.Name = request.FileName;
                    fileInfo.IsDeleted = request.IsDeleted;

                    await questionAudioFilesRepository.UpdateAsync(fileInfo);
                    return true;
                }
                else
                    return false;
            });

            if (response && oldFileName != request.FileName)
                BackgroundJob.Enqueue<IExaminationIntegrationResponseService>(service => service.NotifyLocalInstancesAboutRenameAudioFile(request.FileName, oldFileName));

            return response;
        }

        public async Task<SaveExaminationAudioResult> SaveAsync(Stream stream, string boundary)
        {
            QuestionAudioFile audioFile = new QuestionAudioFile();

            var response = await transactionService.CommitAsync(new[] { TransactionContextScope.Main }, async () =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    audioFile = await GetSavedAudioFileInfo(memoryStream, boundary);
                    if (string.IsNullOrEmpty(audioFile.Name))
                    {
                        return SaveExaminationAudioResult.Error;
                    }
                    await questionAudioFilesRepository.AddAsync(audioFile);
                    return SaveExaminationAudioResult.Ok;
                }
            });

            if (response == SaveExaminationAudioResult.Ok)
                BackgroundJob.Enqueue<IExaminationIntegrationResponseService>(service => service.NotifyLocalInstancesAboutNewAudioFile(CreateFilePath(audioFile.Name)));

            return response;
        }

        private async Task<bool> CheckAudioInfoForUpdate(QuestionAudioFile fileInfo, UpdateExaminationAudioFileInfoRequest request)
        {
            if (fileInfo == null)
                return false;

            if (fileInfo.Name != request.FileName && await questionAudioFilesRepository.ExistsAsync(request.FileName))
                return false;

            if (fileInfo.Id == request.Id && fileInfo.Name == request.FileName && fileInfo.Comment == request.Comment)
                return false;

            return true;
        }

        private async Task<QuestionAudioFile> GetSavedAudioFileInfo(MemoryStream memoryStream, string boundary)
        {
            MultipartSection section;
            var audioFile = new QuestionAudioFile();
            var audioFileInfo = new AddExaminationAudioFileInfoRequest();

            memoryStream.Position = 0;
            var reader = new MultipartReader(boundary, memoryStream);

            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                if (section.HasHeaderInfo("audioFileInfo"))
                {
                    audioFileInfo = await GetAudioFileInfo(section);
                    audioFile = new QuestionAudioFile
                    {
                        Name = audioFileInfo.FileName,
                        Comment = audioFileInfo.Comment
                    };

                    if (await questionAudioFilesRepository.ExistsAsync(audioFile.Name))
                    {
                        return new QuestionAudioFile { Name = string.Empty };
                    }
                }
                if (section.HasHeaderFile() && !string.IsNullOrEmpty(audioFile.Name))
                {
                    var wavFileStream = CreateMemoryStream(section.Body);
                    await SaveAudioFileToLocalFolder(wavFileStream, audioFile.Name);
                }
            }
            audioFile.Path = CreateFilePath(audioFileInfo.FileName);
            return audioFile;
        }

        private async Task SaveAudioFileToLocalFolder(Stream wavFile, string fileName)
        {
            var filePath = CreateFilePath(fileName);
            Directory.CreateDirectory(filesConfiguration.BarneyAudioFilesDirectory);

            wavFile.Position = 0;
            using (var targetStream = File.Create(filePath))
            {
                await wavFile.CopyToAsync(targetStream);
                wavFile.Dispose();
            }
        }

        private string CreateFilePath(string fileName)
        {
            return Path.Combine(filesConfiguration.BarneyAudioFilesDirectory, fileName + filesConfiguration.AudioFileExtention);
        }

        private async Task<AddExaminationAudioFileInfoRequest> GetAudioFileInfo(MultipartSection section)
        {
            using (var streamReader = new StreamReader(section.Body))
            {
                return JsonConvert.DeserializeObject<AddExaminationAudioFileInfoRequest>(await streamReader.ReadToEndAsync());
            }
        }

        private Stream CreateMemoryStream(Stream source)
        {
            var memoryStream = new MemoryStream();
            source.CopyTo(memoryStream);
            return memoryStream;
        }
    }
}
