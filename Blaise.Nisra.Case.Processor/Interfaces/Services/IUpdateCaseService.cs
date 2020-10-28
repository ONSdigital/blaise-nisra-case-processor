using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Nisra.Case.Processor.Interfaces.Services
{
    public interface IUpdateCaseService
    {
        void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string serverPark,
            string surveyName, string serialNumber);
    }
}