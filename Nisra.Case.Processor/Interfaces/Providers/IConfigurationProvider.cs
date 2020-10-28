using System.Collections.Generic;

namespace Nisra.Case.Processor.Interfaces.Providers
{
    public interface IConfigurationProvider
    {
        string ProjectId { get; }

        string SubscriptionId { get; }

        string LocalProcessFolder { get; }

        string CloudProcessedFolder { get; }

        string BucketName { get; }

        IList<string> IgnoreFilesInBucketList { get; }

        string VmName { get; }

        string DeadletterTopicId { get; }
    }
}
