using System.Collections.Generic;

namespace Blaise.Case.Nisra.Processor.Core.Interfaces
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
