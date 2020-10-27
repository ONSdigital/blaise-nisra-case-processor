using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Providers
{
    public interface IConfigurationProvider
    {
        string ProjectId { get; }

        string SubscriptionTopicId { get; }

        string SubscriptionId { get; }

        string LocalProcessFolder { get; }

        string CloudProcessedFolder { get; }

        string BucketName { get; }

        IList<string> IgnoreFilesInBucketList { get; }

        string VmName { get; }

        string DeadletterTopicId { get; }
    }
}
