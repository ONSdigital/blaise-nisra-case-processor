using System.Collections.Generic;
using Blaise.Nisra.Case.Processor.Models;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Nisra.Case.Processor.Interfaces.Mappers
{
    public interface ICaseMapper
    {
        Dictionary<string, string> MapFieldDictionaryFromRecordFields(IDataRecord2 recordData);

        NisraCaseActionModel MapToNisraCaseActionModel(string message);
    }
}
