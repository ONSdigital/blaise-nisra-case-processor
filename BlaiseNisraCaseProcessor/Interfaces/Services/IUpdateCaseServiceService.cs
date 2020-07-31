using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IUpdateCaseServiceService
    {
        void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string serverPark,
            string surveyName, string serialNumber);
    }
}