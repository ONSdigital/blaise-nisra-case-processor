using System.Collections.Generic;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IFieldDataService
    {
        Dictionary<string, string> GetNewCaseFieldData(IDataRecord newDataRecord);
        Dictionary<string, string> GetUpdateCaseFieldData(IDataRecord newDataRecord, IDataRecord existingDataRecord);
    }
}