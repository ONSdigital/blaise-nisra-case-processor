using System.Collections.Generic;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IBlaiseApiService
    {
        IEnumerable<string> GetAvailableServerParks();

        bool SurveyExists(string serverPark, string surveyName);

        string GetSerialNumber(IDataRecord dataRecord);

        string GetCaseId(IDataRecord dataRecord);

        bool CaseExists(string serialNumber, string serverPark, string surveyName);

        IDataSet GetCasesFromFile(string databaseFile);

        void AddDataRecord(IDataRecord dataRecord, string serialNumber, string serverPark, string surveyName);

        IDataRecord GetDataRecord(string serialNumber, string serverPark, string surveyName);

        decimal GetHOutValue(IDataRecord dataRecord);

        void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string serverPark, string surveyName);
    }
}