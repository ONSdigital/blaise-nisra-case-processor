using System.Collections.Generic;
using Blaise.Nuget.Api.Contracts.Enums;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IBlaiseApiService
    {
        IEnumerable<string> GetAvailableServerParks();

        bool SurveyExists(string serverPark, string surveyName);

        string GetSerialNumber(IDataRecord dataRecord);

        bool CaseExists(string serialNumber, string serverPark, string surveyName);

        IDataSet GetCasesFromFile(string databaseFile);

        void AddDataRecord(IDataRecord dataRecord, string serverPark, string surveyName);

        IDataRecord GetDataRecord(string serialNumber, string serverPark, string surveyName);

        bool WebFormStatusFieldExists(IDataRecord dataRecord);

        WebFormStatusType GetWebFormStatus(IDataRecord dataRecord);

        bool HOutFieldExists(IDataRecord dataRecord);
        decimal GetHOutValue(IDataRecord dataRecord);

        void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string serverPark, string surveyName);
    }
}