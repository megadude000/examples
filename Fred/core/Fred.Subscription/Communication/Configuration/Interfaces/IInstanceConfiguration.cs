namespace Company.Fred.Subscription.Communication.Configuration.Interfaces
{
    public interface IInstanceConfiguration
    {
        string Name { get; }
        bool IsSyncEnabled { get; }
    }
}
