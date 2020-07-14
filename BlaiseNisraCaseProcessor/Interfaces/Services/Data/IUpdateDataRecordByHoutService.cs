using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Interfaces.Services.Data
{
    public interface IUpdateDataRecordByHoutService
    {
        void UpdateDataRecordByHoutValues(IDataRecord newDataRecord, IDataRecord existingDataRecord,
            string serverPark, string surveyName, string serialNumber);
    }
}