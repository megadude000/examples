using Company.Gazoo.Database.Entities.Labeling;
using Company.Gazoo.Enumerators.Labeling;
using Company.Gazoo.Responses.Labeling;
using System.Threading.Tasks;

namespace Company.Gazoo.Repositories.Labeling.Interfaces
{
    public interface ILabelGroupsRepository
    {
        Task AddAsync(LabelGroup labelGroup);
        Task UpdateAsync(LabelGroup labelGroup);
        Task<LabelGroupResponse[]> GetAllAsync();
        Task<LabelGroup> GetByIdAsync(long id);
        Task<bool> CheckIfExistsAsync(string name);
        Task<LabelElementType> GetElementTypeAsync(long labelGroupId);
    }
}
