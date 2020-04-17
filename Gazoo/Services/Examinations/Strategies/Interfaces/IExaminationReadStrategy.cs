using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Requests;
using Company.Gazoo.Responses;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Examinations.Strategies.Interfaces
{
    public interface IExaminationReadStrategy
    {
        Task<ExaminationReportInfo[]> GetReportsAsync(ExaminationReportFilter filter);
        Task<AgentForReportResponse[]> GetAgentsForReportsAsync(SearchRequest searchRequest);
    }
}
