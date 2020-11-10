using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IUpdateCaseService
    {
        void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string serverPark,
            string surveyName, string serialNumber);
    }
}