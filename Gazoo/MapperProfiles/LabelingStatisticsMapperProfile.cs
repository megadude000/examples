using AutoMapper;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Responses.Labeling;
using Company.Gazoo.Utils;

namespace Company.Gazoo.MapperProfiles
{
    public class LabelingStatisticsMapperProfile : Profile
    {
        public LabelingStatisticsMapperProfile()
        {

            CreateMap<ImportStatistics, ImportStatisticsResponse>();

            CreateMap<GeneralStatistic, GeneralStatisticModel>()
                .ForMember(dest => dest.ProcessingWorkingTime,
                           src => src.MapFrom(entity => entity.PredictionsWorkingTime + entity.TranscriptionsWorkingTime));

            CreateMap<TranscriptionStatistic, TranscriptionStatisticModel>()
                .ForMember(dest => dest.TranscriptionsProcessingScore,
                           src => src.MapFrom(entity => LabelingStatisticScoreHelper.GetTranscriptionScore(entity.TranscriptionsWorkingTime, entity.TranscribedAudioLength)))
                .ForMember(dest => dest.VerificationsProcessingScore,
                           src => src.MapFrom(entity => LabelingStatisticScoreHelper.GetTranscriptionScore(entity.VerificationsWorkingTime, entity.VerifiedAudiosLength)));

            CreateMap<FCMomentStatistic, FCMomentStatisticModel>()
               .ForMember(dest => dest.PredictionsProcessingScore,
                          src => src.MapFrom(entity => LabelingStatisticScoreHelper.GetPredictionScore(entity.PredictionsWorkingTime, entity.PredictionsCount)))
               .ForMember(dest => dest.VerificationsProcessingScore,
                          src => src.MapFrom(entity => LabelingStatisticScoreHelper.GetPredictionScore(entity.VerificationsWorkingTime, entity.VerificationsCount)));
        }
    }
}
