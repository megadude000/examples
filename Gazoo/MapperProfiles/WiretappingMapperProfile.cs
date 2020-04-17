using AutoMapper;
using Company.BammBamm.Database.Entities.Campaigns;
using Company.Gazoo.Enumerators.Wiretapping;
using Company.Gazoo.Models.Wiretapping;
using Company.Gazoo.Requests.Wiretapping;
using Company.Gazoo.Responses.Wiretapping;

namespace Company.Gazoo.MapperProfiles
{
    public class WiretappingMapperProfile : Profile
    {
        public WiretappingMapperProfile()
        {
            CreateMap<WiretappingFactor, WiretappingFactorModel>()
                .ForMember(dest => dest.AnswerType,
                    src => src.MapFrom(factor => (WiretappingFactorAnswerType)factor.AnswerType))
                .ForMember(dest => dest.SelectedValue,
                    src => src.Ignore());

            CreateMap<AddFactorRequest, WiretappingFactor>();

            CreateMap<WiretappingResultModel, WiretappingResult>()
                .ForMember(dest => dest.CallResultId,
                    src => src.Ignore());

            CreateMap<WiretappingResult, WiretappingResultResponse>()
                .ForMember(dest => dest.AnswerType,
                    src => src.MapFrom(result => (WiretappingFactorAnswerType)result.Factor.AnswerType))
                .ForMember(dest => dest.Id,
                    src => src.MapFrom(result => result.Factor.Id))
                .ForMember(dest => dest.Name,
                    src => src.MapFrom(result => result.Factor.Name))
                .ForMember(dest => dest.SelectedValue,
                    src => src.MapFrom(result => result.Value));
        }
    }
}
