using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IUpdateCaseByHoutService
    {
        void UpdateCaseByHoutValues(IDataRecord newDataRecord, IDataRecord existingDataRecord,
            string serverPark, string surveyName, string serialNumber);
    }
}