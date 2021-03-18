namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IConfigurationProvider
    {
        string ProjectId { get; }

        string SubscriptionId { get; }

        string DeadletterTopicId { get; }

        string LocalTempFolder { get; }
        
        string BucketName { get; }
    }
}
