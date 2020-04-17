using Company.Gazoo.Communication.Contracts.SubscriptionActions;
using Company.Gazoo.Database.Entities;
using Company.Gazoo.Database.Enumerators;
using Company.Gazoo.Responses;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.ApplicationSubscriptions.Interfaces
{
    public interface IApplicationSubscriptionRepository
    {
        Task AddAddressAsync(string address, string instanceName, ApplicationType applicationType);
        Task UpdateStatusAsync(string instanceName, InstanceStatus status);
        Task<long> GetIdByNameAsync(string instanceName);
        Task<InstanceResponse[]> GetExaminationInstancesAsync();
        Task<string[]> GetAliveInstancesAddressesAsync(ApplicationType target);
        Task UpdateAddressAndStatusAsync(SubscriptionRequest request);
        Task<ApplicationInstanceSubscription> GetInfoByNameAsync(string instanceName);
    }
}
