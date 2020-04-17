using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Requests;
using Company.Gazoo.Responses;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Examinations.Interfaces
{
    public interface IExaminationAgentsRepository
    {
        Task AddAsync(Agent agent);
        Task<AgentForReportResponse[]> GetBarneyAgentsAsync(SearchRequest searchRequest);
        Task<AgentForReportResponse[]> GetPebblesAgentsAsync(SearchRequest searchRequest);
        Task<long> GetPebblesAgentIdAsync(long instanceId, long agentId);
    }
}
