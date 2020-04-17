using Company.Gazoo.Communication.Contracts.SubscriptionActions;
using Company.Gazoo.Database.Entities;
using Company.Gazoo.Database.Enumerators;
using Company.Gazoo.DbContexts;
using Company.Gazoo.Repositories.ApplicationSubscriptions.Interfaces;
using Company.Gazoo.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.ApplicationSubscriptions
{
    public class ApplicationSubscriptionRepository : IApplicationSubscriptionRepository
    {
        private readonly GazooContext dbContext;
        private readonly DbSet<ApplicationInstanceSubscription> dbSet;

        public ApplicationSubscriptionRepository(GazooContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<ApplicationInstanceSubscription>();
        }

        public Task AddAddressAsync(string address, string instanceName, ApplicationType applicationType)
        {
            var target = applicationType;
            if (!dbSet.Any(item => item.InstanceName == instanceName))
            {
                dbSet.Add(new ApplicationInstanceSubscription
                {
                    Address = address,
                    InstanceName = instanceName,
                    Target = applicationType,
                    InstanceStatus = InstanceStatus.Alive
                });
            }

            return dbContext.SaveChangesAsync();
        }

        public Task<string[]> GetAliveInstancesAddressesAsync(ApplicationType target)
        {
            return dbSet
                .Where(t => t.InstanceStatus == InstanceStatus.Alive && t.Target == target)
                .Select(gAddress => gAddress.Address)
                .Distinct()
                .ToArrayAsync();
        }

        public Task UpdateStatusAsync(string address, InstanceStatus status)
        {
            var notificationSub = dbSet.SingleOrDefault(n => n.Address == address);
            if (notificationSub != null)
            {
                notificationSub.InstanceStatus = status;
                dbSet.Update(notificationSub);
            }
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAddressAndStatusAsync(SubscriptionRequest request)
        {
            var notificationSub = dbSet.SingleOrDefault(n => n.InstanceName == request.InstanceName);
            if (notificationSub != null)
            {
                notificationSub.InstanceStatus = InstanceStatus.Alive;
                notificationSub.Address = request.Endpoint;
                notificationSub.Target = (ApplicationType)request.Target;
                dbSet.Update(notificationSub);
            }
            return dbContext.SaveChangesAsync();
        }

        public Task<InstanceResponse[]> GetExaminationInstancesAsync()
        {
            return dbSet
                   .Where(gAddress => gAddress.Target == ApplicationType.Examination || gAddress.Target == ApplicationType.LocalGazoo)
                   .Where(gAddress => gAddress.InstanceStatus == InstanceStatus.Alive)
                   .Select(gAddress => new InstanceResponse { Id = gAddress.Id, Name = gAddress.InstanceName })
                   .ToArrayAsync();
        }

        public Task<long> GetIdByNameAsync(string instanceName)
        {
            return dbSet
                    .Where(gAddress => gAddress.InstanceName == instanceName)
                    .Select(gAddress => gAddress.Id)
                    .FirstOrDefaultAsync();
        }

        public Task<ApplicationInstanceSubscription> GetInfoByNameAsync(string instanceName)
        {
            return dbSet
                .Where(instance => instance.InstanceName == instanceName)
                .FirstOrDefaultAsync();
        }
    }
}