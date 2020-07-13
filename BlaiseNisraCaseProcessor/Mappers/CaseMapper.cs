using System.Collections.Generic;
using BlaiseNisraCaseProcessor.Enums;
using BlaiseNisraCaseProcessor.Helpers;
using BlaiseNisraCaseProcessor.Interfaces;
using Newtonsoft.Json;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Mappers
{
    public class CaseMapper : ICaseMapper
    {
        public string MapToSerializedJson(IDataRecord recordData, string instrumentName, string serverPark, string primaryKey, CaseStatusType caseStatusType)
        {
            var jsonData = new Dictionary<string, string>();

            if (recordData != null)
            {
                foreach (var qidField in recordData.GetField("QID").Fields)
                {
                    jsonData[qidField.LocalName] = qidField.DataValue.ValueAsText.ToLower();
                }
            }

            jsonData["primary_key"] = primaryKey;
            jsonData["instrument_name"] = instrumentName;
            jsonData["server_park"] = serverPark;
            jsonData["status"] = EnumHelper.GetEnumDescription(caseStatusType);

            return JsonConvert.SerializeObject(jsonData);
        }
    }
}
