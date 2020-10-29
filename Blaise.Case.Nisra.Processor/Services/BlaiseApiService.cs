using System.Collections.Generic;
using Blaise.Case.Nisra.Processor.Interfaces.Mappers;
using Blaise.Case.Nisra.Processor.Interfaces.Services;
using Blaise.Nuget.Api.Contracts.Enums;
using Blaise.Nuget.Api.Contracts.Interfaces;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Services
{
    public class BlaiseApiService : IBlaiseApiService
    {
        private readonly IBlaiseApi _blaiseApi;
        private readonly ICaseMapper _mapper;

        public BlaiseApiService(
            IBlaiseApi blaiseApi,
            ICaseMapper mapper)
        {
            _blaiseApi = blaiseApi;
            _mapper = mapper;
        }

        public IEnumerable<string> GetAvailableServerParks()
        {
            return _blaiseApi.GetServerParkNames(_blaiseApi.GetDefaultConnectionModel());
        }

        public bool SurveyExists(string serverParkName, string instrumentName)
        {
            return _blaiseApi.SurveyExists(_blaiseApi.GetDefaultConnectionModel(), instrumentName, serverParkName);
        }

        public string GetSerialNumber(IDataRecord dataRecord)
        {
            return _blaiseApi.GetPrimaryKeyValue(dataRecord);
        }

        public bool CaseExists(string serialNumber, string serverParkName, string instrumentName)
        {
            return _blaiseApi.CaseExists(_blaiseApi.GetDefaultConnectionModel(), serialNumber, 
                instrumentName, serverParkName);
        }

        public IDataSet GetCasesFromFile(string databaseFile)
        {
            return _blaiseApi.GetDataSet(databaseFile);
        }

        public void AddDataRecord(IDataRecord dataRecord, string serialNumber, string serverParkName, string instrumentName)
        {
            var fieldData = _mapper.MapFieldDictionaryFromRecordFields(dataRecord as IDataRecord2);

            _blaiseApi.CreateNewDataRecord(_blaiseApi.GetDefaultConnectionModel(), serialNumber, fieldData,
                    instrumentName, serverParkName);
        }

        public IDataRecord GetDataRecord(string serialNumber, string serverParkName, string instrumentName)
        {
            return _blaiseApi.GetDataRecord(_blaiseApi.GetDefaultConnectionModel(), serialNumber,
                    instrumentName, serverParkName);
        }

        public decimal GetHOutValue(IDataRecord dataRecord)
        {
            var dataValue = _blaiseApi.GetFieldValue(dataRecord, FieldNameType.HOut);

            return dataValue.IntegerValue;
        }

        public void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string serverParkName, 
            string instrumentName)
        {
            var fieldData = _mapper.MapFieldDictionaryFromRecordFields(newDataRecord as IDataRecord2);

            // Modify the Online flag to indicate the new record is from the NISRA data set
            fieldData.Add("QHAdmin.Online", "1");

            _blaiseApi.UpdateDataRecord(_blaiseApi.GetDefaultConnectionModel(), existingDataRecord, fieldData,
                    instrumentName, serverParkName);
        }
    }
}
