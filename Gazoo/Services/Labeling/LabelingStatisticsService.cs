using Company.Gazoo.Extensions;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Models.User;
using Company.Gazoo.Repositories.Labeling.Interfaces;
using Company.Gazoo.Requests;
using Company.Gazoo.Requests.Labeling;
using Company.Gazoo.Responses.Labeling;
using Company.Gazoo.Services.Labeling.Interfaces;
using Company.Gazoo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Company.Gazoo.Database.Entities.Enums;

namespace Company.Gazoo.Services.Labeling
{
    internal class LabelingStatisticsService : ILabelingStatisticsService
    {
        private readonly IUserService userService;
        private readonly IImportRepository importsRepository;
        private readonly ILabelingStatisticRepository labelingStatisticRepository;

        public LabelingStatisticsService(IUserService userService,
            IImportRepository importsRepository,
            ILabelingStatisticRepository labelingStatisticRepository)
        {
            this.userService = userService;
            this.importsRepository = importsRepository;
            this.labelingStatisticRepository = labelingStatisticRepository;
        }
        public async Task<GeneralLabelingStatisticResponse> GetGeneralStatisticsAsync(LabelingStatisticRequest request)
        {
            var agentIds = await GetAgentsForStatistics(request);

            var generalStatistics = await labelingStatisticRepository.GetGeneralStatisticsAsync(agentIds, request.FromDate, request.ToDate);

            if (!generalStatistics.Any())
                return new GeneralLabelingStatisticResponse();

            return new GeneralLabelingStatisticResponse
            {
                Statistic = generalStatistics,
                AggregatedStatistic = FormGeneralAggregatedStatistic(generalStatistics)
            };
        }

        public async Task<TranscriptionStatisticsResponse> GetTranscriptionStatisticsAsync(LabelingStatisticRequest request)
        {
            var agentIds = await GetAgentsForStatistics(request);

            var transcriptionsStatistics = await labelingStatisticRepository.GetTranscriptionStatisticsAsync(agentIds, request.FromDate, request.ToDate);

            if (!transcriptionsStatistics.Any())
                return new TranscriptionStatisticsResponse();

            return new TranscriptionStatisticsResponse
            {
                Statistic = transcriptionsStatistics,
                AggregatedStatistic = FormTranscriptionAggregatedStatistic(transcriptionsStatistics)
            };
        }

        public async Task<FCMomentStatisticsResponse> GetFCMomentStatisticsAsync(LabelingStatisticRequest request)
        {
            var agentIds = await GetAgentsForStatistics(request);

            var fcMomentsStatistics = await labelingStatisticRepository.GetFCMomentStatisticsAsync(agentIds, request.FromDate, request.ToDate);

            if (!fcMomentsStatistics.Any())
                return new FCMomentStatisticsResponse();

            return new FCMomentStatisticsResponse
            {
                Statistic = fcMomentsStatistics,
                AggregatedStatistic = FormFCMomentAggregatedStatistic(fcMomentsStatistics)
            };
        }

        public async Task<ImportStatisticsResponse[]> GetImportStatisticsAsync(GetImportsReportRequest reqest)
        {
            var result = new List<ImportStatisticsResponse>();

            if (reqest.SelectedTypes.Contains(LabelingType.Transcription))
                result.AddRange(await GetTranscriptionStatistics(reqest));

            if (reqest.SelectedTypes.Contains(LabelingType.FullConversationMoments))
                result.AddRange(await importsRepository.GetFCMomentStatisticsAsync());

            return result.Where(item => item.UploadedRecords > 0).OrderBy(item => item.Id).ToArray();
        }

        public async Task<UserModel[]> GetAgentsForReportsAsync(SearchRequest searchRequest)
        {
            var users = await userService.GetUsersWithClaimAsync(CustomClaims.ConversationTranscriber);

            return users
                .Where(agent => string.IsNullOrWhiteSpace(searchRequest.SearchString) || Regex.IsMatch($"{agent.GivenName} {agent.Surname}", searchRequest.SearchString, RegexOptions.IgnoreCase))
                .Take(searchRequest.ResultsLimit)
                .ToArray();
        }

        private async Task<ImportStatisticsResponse[]> GetTranscriptionStatistics(GetImportsReportRequest reqest)
        {
            var transcriptionStatistics = await importsRepository.GetTranscriptionStatisticsAsync();

            if (reqest.SelectedInstanceId.HasValue)
                transcriptionStatistics = transcriptionStatistics.Where(item => item.InstanceId == reqest.SelectedInstanceId.Value).ToArray();

            if (reqest.SelectedCampaignsIds.Any())
                transcriptionStatistics = transcriptionStatistics.Where(item => reqest.SelectedCampaignsIds.Any(campaingId => campaingId == item.CampaignId)).ToArray();

            return transcriptionStatistics;
        }

        private AggregatedGeneralStatistic FormGeneralAggregatedStatistic(IReadOnlyCollection<GeneralStatisticModel> statistics)
        {
            return new AggregatedGeneralStatistic
            {
                TotalPredictionsCount = statistics.Sum(item => item.PredictionsCount),
                TotalPredictionsWorkingTime = new TimeSpan(statistics.Sum(item => item.PredictionsWorkingTime.Ticks)),
                TotalTranscriptionsWorkingTime = new TimeSpan(statistics.Sum(item => item.TranscriptionsWorkingTime.Ticks)),
                TotalTranscriptionsCount = statistics.Sum(item => item.TranscriptionsCount),
                TotalProcessingWorkingTime = new TimeSpan(statistics.Sum(item => item.ProcessingWorkingTime.Ticks))
            };
        }

        private AggregatedTranscriptionStatistic FormTranscriptionAggregatedStatistic(IReadOnlyCollection<TranscriptionStatisticModel> statistics)
        {
            return new AggregatedTranscriptionStatistic
            {
                TotalTranscriptionsProcessingScore = CountTotalScore(statistics.Where(item => item.TranscriptionsProcessingScore != 0).Select(item => item.TranscriptionsProcessingScore).ToArray()),
                TotalVerificationsProcessingScore = CountTotalScore(statistics.Where(item => item.VerificationsProcessingScore != 0).Select(item => item.VerificationsProcessingScore).ToArray()),
                TotalTranscribedAudioLength = new TimeSpan(statistics.Sum(item => item.TranscribedAudioLength.Ticks)),
                TotalVerifiedAudiosLength = new TimeSpan(statistics.Sum(item => item.VerifiedAudiosLength.Ticks)),
                TotalTranscriptionsCount = statistics.Sum(item => item.TranscriptionsCount),
                TotalVerificationsCount = statistics.Sum(item => item.VerificationsCount),
                TotalTranscriptionsWorkingTime = new TimeSpan(statistics.Sum(item => item.TranscriptionsWorkingTime.Ticks)),
                TotalVerificationsWorkingTime = new TimeSpan(statistics.Sum(item => item.VerificationsWorkingTime.Ticks)),
                TotalAverageWordErrorRate = statistics.Average(item => item.AverageWordErrorRate)
            };
        }

        private AggregatedFCMomentStatistic FormFCMomentAggregatedStatistic(IReadOnlyCollection<FCMomentStatisticModel> statistics)
        {
            return new AggregatedFCMomentStatistic
            {
                TotalPredictionsProcessingScore = CountTotalScore(statistics.Where(item => item.PredictionsProcessingScore != 0).Select(item => item.PredictionsProcessingScore).ToArray()),
                TotalVeriricationsProcessingScore = CountTotalScore(statistics.Where(item => item.VerificationsProcessingScore != 0).Select(item => item.VerificationsProcessingScore).ToArray()),
                TotalPredictionsCount = statistics.Sum(item => item.PredictionsCount),
                TotalVerificationsCount = statistics.Sum(item => item.VerificationsCount),
                TotalPredictionsWorkingTime = new TimeSpan(statistics.Sum(item => item.PredictionsWorkingTime.Ticks)),
                TotalVerificationsWorkingTime = new TimeSpan(statistics.Sum(item => item.VerificationsWorkingTime.Ticks)),
            };
        }

        private double CountTotalScore(double[] scores)
        {
            if (!scores.Any())
                return 0;

            var totalScore = scores.Sum() / scores.Length;

            if (double.IsNaN(totalScore) || double.IsInfinity(totalScore))
                return 0;

            return Math.Round(totalScore, 2);
        }

        private async Task<long[]> GetAgentsForStatistics(LabelingStatisticRequest request)
        {
            return request.AgentIds.Any()
                ? request.AgentIds
                : (await userService.GetUsersWithClaimAsync(CustomClaims.ConversationTranscriber)).Select(user => user.Id).ToArray();
        }
    }
}
