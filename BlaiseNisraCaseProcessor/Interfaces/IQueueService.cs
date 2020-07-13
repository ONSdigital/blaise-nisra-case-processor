
namespace BlaiseNisraCaseProcessor.Interfaces
{
    public interface IQueueService
    {
        void PublishMessage(string message);
    }
}