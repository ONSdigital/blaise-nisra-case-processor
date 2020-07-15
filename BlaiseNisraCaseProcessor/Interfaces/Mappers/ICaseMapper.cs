using System.Collections.Generic;
using BlaiseNisraCaseProcessor.Enums;
using BlaiseNisraCaseProcessor.Models;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Interfaces.Mappers
{
    public interface ICaseMapper
    {
        string MapToSerializedJson(IDataRecord recordData, string surveyName, string serverPark, CaseStatusType caseStatusType);

        Dictionary<string, string> MapFieldDictionaryFromRecordFields(IDataRecord2 recordData);

        NisraCaseActionModel MapToNisraCaseActionModel(string message);
    }
}
