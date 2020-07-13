
namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IQueueService
    {
        void PublishMessage(string message);
    }
}