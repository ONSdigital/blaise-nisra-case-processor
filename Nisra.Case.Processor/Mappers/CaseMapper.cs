using System.Collections.Generic;
using Newtonsoft.Json;
using Nisra.Case.Processor.Enums;
using Nisra.Case.Processor.Interfaces.Mappers;
using Nisra.Case.Processor.Models;
using StatNeth.Blaise.API.DataRecord;

namespace Nisra.Case.Processor.Mappers
{
    public class CaseMapper : ICaseMapper
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

        public NisraCaseActionModel MapToNisraCaseActionModel(string message)
        {
            try
            {
                return JsonConvert.DeserializeObject<NisraCaseActionModel>(message);
            }
            catch 
            {
                // This is horrible I know but we currently don't really care about the message as it is only a trigger
                // and we need to ensure a message incorrectly put on this topic does not trigger it
            }

            return new NisraCaseActionModel { Action = ActionType.NotSupported };
        }
    }
}
