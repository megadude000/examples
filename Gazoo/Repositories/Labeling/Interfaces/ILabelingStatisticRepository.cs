using Company.Gazoo.Models.Labeling;
using System;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface ILabelingStatisticRepository
    {
        Task<GeneralStatisticModel[]> GetGeneralStatisticsAsync(long[] agentIds, DateTime fromDate, DateTime toDate);
        Task<TranscriptionStatisticModel[]> GetTranscriptionStatisticsAsync(long[] agentIds, DateTime fromDate, DateTime toDate);
        Task<FCMomentStatisticModel[]> GetFCMomentStatisticsAsync(long[] agentIds, DateTime fromDate, DateTime toDate);
    }
}
