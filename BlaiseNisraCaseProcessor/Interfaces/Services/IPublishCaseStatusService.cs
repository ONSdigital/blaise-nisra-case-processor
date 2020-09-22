using Blaise.Nuget.Api.Contracts.Enums;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IPublishCaseStatusService
    {
        void PublishCaseStatus(IDataRecord recordData, string surveyName, string serverPark, CaseStatusType caseStatusType);
    }
}