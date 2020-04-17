using Company.Gazoo.Database.Entities.Enums;

namespace Company.Gazoo.Services.Labeling.Interfaces
{
    public interface IAudioReopenSubscriptionService
    {
        void Subscribe(long id, LabelingType type);
        void Unsubscribe(long id, LabelingType type);
    }
}
