using AutoMapper;
using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Models.Labeling;
using Company.Gazoo.Requests.Labeling;
using Company.Gazoo.Responses.Labeling;
using System.Linq;

namespace Company.Gazoo.MapperProfiles
{
    public class LabelingMapperProfile : Profile
    {
        public LabelingMapperProfile()
        {
            CreateMap<ClientUtteranceRequest, ClientUtterance>()
                .ForMember(transcription => transcription.CallId, options => options.Ignore())
                .ForMember(transcription => transcription.AuthorId, options => options.Ignore());

            CreateMap<ClientUtterance, ClientUtteranceResponse>()
                .ForMember(transcription => transcription.SelectedLabels,
                    m => m.MapFrom(entity => entity.SelectedLabel
                        .Select(label => label.LabelId)
                        .ToArray()));

            CreateMap<AudioMessageLogUpdateRequest, AudioMessageLogUpdate>()
                .ForMember(messageLogUpdate => messageLogUpdate.CallId, options => options.Ignore())
                .ForMember(transcription => transcription.AuthorId, options => options.Ignore());

            CreateMap<AudioMessageLogUpdate, AudioMessageLogUpdateResponse>();

            CreateMap<AddLabelGroupRequest, LabelGroup>();

            CreateMap<UpdateLabelGroupRequest, LabelGroup>();

            CreateMap<AddTranscriptionLabelRequest, TranscriptionLabel>();

            CreateMap<UpdateTranscriptionLabelRequest, TranscriptionLabel>();

            CreateMap<LabelGroup, LabelGroupResponse>();

            CreateMap<Transcription, TranscriptionModel>();

            CreateMap<LabelGroup, LabelGroupModel>();

            CreateMap<TranscriptionLabel, TranscriptionLabelModel>();

            CreateMap<TranscriptionModel, Transcription>();

            CreateMap<FCMoment, FCMomentModel>();

            CreateMap<FCMomentModel, FCMoment>();

            CreateMap<FCMomentAMLog, FCMomentAMLogModel>();

            CreateMap<FCMomentAMLogModel, FCMomentAMLog>();
        }
    }
}
