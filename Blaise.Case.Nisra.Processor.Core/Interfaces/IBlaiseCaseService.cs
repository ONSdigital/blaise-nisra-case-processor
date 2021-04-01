using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IBlaiseCaseService
    {
        void CreateCase(IDataRecord dataRecord, string instrumentName, string serverParkName,
            string primaryKey);

        void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string instrumentName,
            string serverParkName, string primaryKey);
    }
}