using System.Collections.Generic;
using Blaise.Case.Nisra.Processor.Data.Interfaces;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Data.Mappers
{
    public class DataRecordMapper : IDataRecordMapper
    {
        public Dictionary<string, string> MapFieldDictionaryFromRecordFields(IDataRecord2 recordData)
        {
            var fieldDictionary = new Dictionary<string, string>();
            var dataFields = recordData.GetDataFields();

            foreach (var dataField in dataFields)
            {
                fieldDictionary[dataField.FullName] = dataField.DataValue.ValueAsText;
            }

            return fieldDictionary;
        }
    }
}
