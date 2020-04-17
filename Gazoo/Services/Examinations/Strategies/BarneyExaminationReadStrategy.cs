using AutoMapper;
using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Repositories.Examinations.Interfaces;
using Company.Gazoo.Requests;
using Company.Gazoo.Responses;
using Company.Gazoo.Services.Examinations.Strategies.Interfaces;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations.Strategies
{
    public class BarneyExaminationReadStrategy : IExaminationReadStrategy
    {
        private readonly IExaminationAgentsRepository examinationAgentsRepository;
        private readonly IExaminationResultsRepository examinationResultsRepository;

        public BarneyExaminationReadStrategy(IExaminationAgentsRepository examinationAgentsRepository,
            IExaminationResultsRepository examinationResultsRepository)
        {
            this.examinationAgentsRepository = examinationAgentsRepository;
            this.examinationResultsRepository = examinationResultsRepository;
        }

        public async Task<AgentForReportResponse[]> GetAgentsForReportsAsync(SearchRequest searchRequest)
            => await examinationAgentsRepository.GetBarneyAgentsAsync(searchRequest);

        public async Task<ExaminationReportInfo[]> GetReportsAsync(ExaminationReportFilter filter)
            => await examinationResultsRepository.GetBarneyResultsAsync(filter);
    }
}
