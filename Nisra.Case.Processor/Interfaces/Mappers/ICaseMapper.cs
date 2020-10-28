using System.Collections.Generic;
using Nisra.Case.Processor.Models;
using StatNeth.Blaise.API.DataRecord;

namespace Nisra.Case.Processor.Interfaces.Mappers
{
    public interface ICaseMapper
    {
        Dictionary<string, string> MapFieldDictionaryFromRecordFields(IDataRecord2 recordData);

        NisraCaseActionModel MapToNisraCaseActionModel(string message);
    }
}
