
namespace BlaiseNisraCaseProcessor.Interfaces.Providers
{
    public interface IConfigurationProvider
    {
        string ProjectId { get; }

        string TopicId { get; }

        string BucketName { get; }

        string CloudStorageKey { get; }
    }
}
