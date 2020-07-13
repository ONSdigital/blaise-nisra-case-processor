using BlaiseNisraCaseProcessor.Enums;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Interfaces.Mappers
{
    public interface ICaseMapper
    {
        string MapToSerializedJson(IDataRecord recordData, string instrumentName, string serverPark, string primaryKey, CaseStatusType caseStatusType);
    }
}
