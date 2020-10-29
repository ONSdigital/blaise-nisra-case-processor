using System.Collections.Generic;
using Blaise.Case.Nisra.Processor.Models;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Interfaces.Mappers
{
    public interface ICaseMapper
    {
        Dictionary<string, string> MapFieldDictionaryFromRecordFields(IDataRecord2 recordData);

        NisraCaseActionModel MapToNisraCaseActionModel(string message);
    }
}
