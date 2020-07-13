using BlaiseNisraCaseProcessor.Enums;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Interfaces
{
    public interface IPublishCaseStatusService
    {
        void PublishCaseStatus(IDataRecord recordData, string instrumentName, string serverPark, string primaryKey, CaseStatusType caseStatusType);
    }
}