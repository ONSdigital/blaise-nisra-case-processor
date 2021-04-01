using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IImportNisraCaseService
    {
        void ImportNisraCase(IDataRecord nisraDataRecord, string instrumentName, string serverParkName);
    }
}