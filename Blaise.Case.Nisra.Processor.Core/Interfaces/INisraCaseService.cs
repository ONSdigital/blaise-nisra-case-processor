using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface INisraCaseService
    {
        void CreateOnlineCase(IDataRecord dataRecord, string instrumentName, string serverParkName, 
            string primaryKey);

        void UpdateExistingCaseWithOnlineData(IDataRecord nisraDataRecord, IDataRecord existingDataRecord, string serverParkName, 
            string instrumentName, string primaryKey);
    }
}