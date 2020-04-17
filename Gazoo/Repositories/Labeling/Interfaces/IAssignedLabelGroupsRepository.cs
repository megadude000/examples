using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Models.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface IAssignedLabelGroupsRepository
    {
        Task AddRangeAsync(AssignedLabelGroups[] assignedLabels);
        Task<LabelGroupModel[]> GetAsync(long importNumber);
    }
}
