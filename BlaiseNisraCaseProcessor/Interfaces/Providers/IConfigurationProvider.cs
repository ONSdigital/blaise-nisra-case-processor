using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Providers
{
    public interface IConfigurationProvider
    {
        string ProjectId { get; }

        string TopicId { get; }

        string LocalProcessFolder { get; }

        string BucketName { get; }

        IList<string> IgnoreFilesInBucketList { get; }

        string CloudStorageKey { get; }
    }
}
