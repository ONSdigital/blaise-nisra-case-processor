using System.Collections.Generic;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Data.Interfaces
{
    public interface IDataRecordMapper
    {
        Dictionary<string, string> MapFieldDictionaryFromRecordFields(IDataRecord2 recordData);
    }
}
