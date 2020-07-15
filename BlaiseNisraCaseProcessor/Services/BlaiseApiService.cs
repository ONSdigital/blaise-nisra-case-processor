using System.Collections.Generic;
using Blaise.Nuget.Api.Contracts.Enums;
using Blaise.Nuget.Api.Contracts.Interfaces;
using BlaiseNisraCaseProcessor.Interfaces.Mappers;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Services
{
    public class BlaiseApiService : IBlaiseApiService
    {
        private readonly IFluentBlaiseApi _blaiseApi;
        private readonly ICaseMapper _mapper;

        public BlaiseApiService(
            IFluentBlaiseApi blaiseApi, 
            ICaseMapper mapper)
        {
            _blaiseApi = blaiseApi;
            _mapper = mapper;
        }

        public IEnumerable<string> GetAvailableServerParks()
        {
            return _blaiseApi
                .WithConnection(_blaiseApi.DefaultConnection)
                .ServerParks;
        }

        public bool SurveyExists(string serverPark, string surveyName)
        {
            return _blaiseApi
                .WithConnection(_blaiseApi.DefaultConnection)
                .WithServerPark(serverPark)
                .WithInstrument(surveyName)
                .Survey
                .Exists;
        }

        public string GetSerialNumber(IDataRecord dataRecord)
        {
            return _blaiseApi.Case.WithDataRecord(dataRecord).PrimaryKey;
        }

        public bool CaseExists(string serialNumber, string serverPark, string surveyName)
        {
            return _blaiseApi
                .WithConnection(_blaiseApi.DefaultConnection)
                .WithServerPark(serverPark)
                .WithInstrument(surveyName)
                .Case
                .WithPrimaryKey(serialNumber)
                .Exists;
        }

        public IDataSet GetCasesFromFile(string databaseFile)
        {
            return _blaiseApi
                .WithConnection(_blaiseApi.DefaultConnection)
                .WithFile(databaseFile)
                .Cases;
        }

        public void AddDataRecord(IDataRecord dataRecord, string serverPark, string surveyName)
        {
            _blaiseApi
                .WithConnection(_blaiseApi.DefaultConnection)
                .WithServerPark(serverPark)
                .WithInstrument(surveyName)
                .Case
                .WithDataRecord(dataRecord)
                .Add();
        }

        public IDataRecord GetDataRecord(string serialNumber, string serverPark, string surveyName)
        {
            return _blaiseApi
                .WithConnection(_blaiseApi.DefaultConnection)
                .WithServerPark(serverPark)
                .WithInstrument(surveyName)
                .Case
                .WithPrimaryKey(serialNumber)
                .Get();
        }

        public bool WebFormStatusFieldExists(IDataRecord dataRecord)
        {
            return _blaiseApi
                .Case
                .WithDataRecord(dataRecord)
                .HasField(FieldNameType.WebFormStatus);
        }

        public WebFormStatusType GetWebFormStatus(IDataRecord dataRecord)
        {
            return _blaiseApi
                .Case
                .WithDataRecord(dataRecord)
                .WebFormStatus;
        }

        public bool HOutFieldExists(IDataRecord dataRecord)
        {
            return _blaiseApi
                .Case
                .WithDataRecord(dataRecord)
                .HasField(FieldNameType.HOut);
        }

        public decimal GetHOutValue(IDataRecord dataRecord)
        {
            return _blaiseApi
                .Case
                .WithDataRecord(dataRecord)
                .HOut;
        }

        public void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string serverPark, string surveyName)
        {
            var fieldData = _mapper.MapFieldDictionaryFromRecordFields(newDataRecord as IDataRecord2);

            // Modify the Online flag to indicate the new record is from the NISRA data set
            fieldData.Add("QHAdmin.Online", "1");

            _blaiseApi
                .WithConnection(_blaiseApi.DefaultConnection)
                .WithServerPark(serverPark)
                .WithInstrument(surveyName)
                .Case
                .WithDataRecord(existingDataRecord)
                .WithData(fieldData)
                .Update();
        }
    }
}
