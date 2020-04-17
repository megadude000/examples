using AutoMapper;
using Company.AspNet.Identity.Extensions;
using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Models.Examinations.RequestModels;
using Company.Gazoo.Requests;
using Company.Gazoo.Responses;
using Company.Gazoo.Utils;
using System;
using System.Linq;

namespace Company.Gazoo.MapperProfiles
{
    public class ExaminationMapperProfile : Profile
    {
        public ExaminationMapperProfile()
        {
            CreateMap<ExaminationStatistic, ExaminationStatisticModel>();

            CreateMap<Examination, ExaminationForReportResponse>();

            CreateMap<QuestionAudioFile, AudioFileResponse>();

            CreateMap<AddQuestionRequest, Question>();

            CreateMap<UpdateQuestionRequest, Question>();

            CreateMap<AddExaminationRequest, Examination>()
                .ForMember(answer => answer.CreationTime,
                    m => m.MapFrom(request => DateTime.UtcNow))
                .ForMember(answer => answer.ModificationTime,
                    m => m.MapFrom(request => DateTime.UtcNow));

            CreateMap<Examination, ExaminationInfoResponse>();

            CreateMap<AddAnswerRequest, Answer>()
                .ForMember(answer => answer.PredefinedAnswerSetId,
                    m => m.MapFrom(request => request.SetId));

            CreateMap<Answer, AnswerResponse>();

            CreateMap<PredefinedAnswerSet, PredefinedAnswerSetResponse>();

            CreateMap<ExaminationReportsInformationModel, ExaminationResult>();

            CreateMap<Examination, ExaminationForReportResponse>();

            CreateMap<Examination, ExaminationModel>();

            CreateMap<Examination, ExaminationResponse>()
                .ForMember(response => response.AuthorName,
                    m => m.MapFrom(item => item.Author.GetFullName()));

            CreateMap<ExaminationBarneyReportsInformationModel, ExaminationResult>()
                .ForMember(examResult => examResult.ExaminationId,
                    m => m.MapFrom(report => report.ExaminationData.Key));

            CreateMap<Answer, ExaminationAnswerModel>()
                .ForMember(answerModel => answerModel.Value,
                    m => m.MapFrom(answer => answer.Name));

            CreateMap<Agent, AgentForReportResponse>()
                .ForMember(agentForReport => agentForReport.Name,
                    m => m.MapFrom(agent => $"{agent.Surname} {agent.Name}"))
                .ForMember(agentForReport => agentForReport.InstanceName,
                    m => m.MapFrom(agent => (agent.Instance == null) ? "" : agent.Instance.InstanceName))
                .ForMember(agentForReport => agentForReport.InstanceId,
                    m => m.MapFrom(agent => (agent.Instance == null) ? default : agent.Instance.Id));

            CreateMap<ExaminationReportsInformationModel, Agent>()
               .ForMember(agent => agent.Name,
                   m => m.MapFrom(report => report.UserInformation.FirstName))
               .ForMember(agent => agent.Surname,
                   m => m.MapFrom(report => report.UserInformation.SecondName))
               .ForPath(agent => agent.InstanceId,
                   m => m.MapFrom(report => report.InstanceId))
               .ForPath(agent => agent.LocalAgentId,
                   m => m.MapFrom(report => report.UserInformation.Id));

            CreateMap<Question, QuestionResponse>()
                .ForMember(dest => dest.AudioFileName,
                    m => m.MapFrom(src => src.QuestionAudioFile.Name))
                 .ForMember(dest => dest.AudioFileId,
                    m => m.MapFrom(src => src.QuestionAudioFileId))
                .ForMember(dest => dest.Answers,
                    m => m.MapFrom(src => ExaminationAnswerHelper.FormAnswers(src.QuestionAnswers)));

            CreateMap<ExaminationResult, ExaminationReportInfo>()
                .ForMember(examReport => examReport.InstanceName,
                    m => m.MapFrom(result => result.Instance.InstanceName))
                .ForMember(examReport => examReport.ExaminationStartDate,
                    m => m.MapFrom(result => result.StartDate))
                .ForMember(examReport => examReport.ExaminationName,
                    m => m.MapFrom(result => result.Examination.Name))
                .ForMember(examReport => examReport.ExaminationEndDate,
                    m => m.MapFrom(result => result.EndDate))
                .ForMember(examReport => examReport.AgentName,
                    m => m.MapFrom(result => $"{result.Agent.Surname} {result.Agent.Name}"));

            CreateMap<Question, ExaminationQuestionModel>()
               .ForMember(questionModel => questionModel.AudioFileName,
                   m => m.MapFrom(item => item.QuestionAudioFile.Name))
               .ForMember(questionModel => questionModel.ExpectedAnswers,
                   m => m.MapFrom(item => ExaminationAnswerHelper.FormExpectedAnswers(item.QuestionAnswers)))
               .ForMember(questionModel => questionModel.DefaultReactionTime,
                   m => m.MapFrom(item => item.PerfectReactionTime))
               .ForMember(questionModel => questionModel.QuestionId,
                   m => m.MapFrom(item => item.Id))
               .ForMember(questionModel => questionModel.MaxReactionTime,
                   m => m.MapFrom(item => item.Examination.ReactionTime))
               .ForMember(questionModel => questionModel.IsRandom,
                   m => m.MapFrom(item => item.Examination.IsRandom));

            CreateMap<QuestionResult, QuestionResultResponse>()
              .ForMember(questionResponse => questionResponse.ReactionTime,
                  m => m.MapFrom(item => (item.ReactionTime.TotalMinutes < 1) ? item.ReactionTime.ToString(@"s\.ff") : item.ReactionTime.ToString(@"m\:ss\.ff")))
              .ForMember(questionResponse => questionResponse.SelectedAnswer,
                  m => m.MapFrom(item => (item.QuestionResultAnswers.Any()) ? ExaminationAnswerHelper.FormSelectedAnswer(item.QuestionResultAnswers) : null))
              .ForMember(questionResponse => questionResponse.Answers,
                  m => m.MapFrom(item => ExaminationAnswerHelper.FormAnswers(item.Question.QuestionAnswers)));
        }
    }
}
