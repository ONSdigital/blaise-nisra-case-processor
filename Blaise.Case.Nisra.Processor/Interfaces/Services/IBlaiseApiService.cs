using System.Collections.Generic;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Interfaces.Services
{
    public interface IBlaiseApiService
    {
        IEnumerable<string> GetAvailableServerParks();

        bool SurveyExists(string serverParkName, string instrumentName);

        string GetSerialNumber(IDataRecord dataRecord);

        bool CaseExists(string serialNumber, string serverParkName, string instrumentName);

        IDataSet GetCasesFromFile(string databaseFile);

        void AddDataRecord(IDataRecord dataRecord, string serialNumber, string serverParkName, string instrumentName);

        IDataRecord GetDataRecord(string serialNumber, string serverParkName, string instrumentName);

        decimal GetHOutValue(IDataRecord dataRecord);

        void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string serverParkName, string instrumentName);
    }
}