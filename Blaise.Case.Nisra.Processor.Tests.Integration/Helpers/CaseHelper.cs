using System;
using System.Collections.Generic;
using System.Globalization;
using Blaise.Case.Nisra.Processor.Tests.Integration.Enums;
using Blaise.Case.Nisra.Processor.Tests.Integration.Models;
using Blaise.Nuget.Api.Api;
using Blaise.Nuget.Api.Contracts.Enums;
using Blaise.Nuget.Api.Contracts.Extensions;
using Blaise.Nuget.Api.Contracts.Interfaces;

namespace Blaise.Case.Nisra.Processor.Tests.Integration.Helpers
{
    public class CaseHelper
    {
        private readonly IBlaiseCaseApi _blaiseCaseApi;

        private static CaseHelper _currentInstance;

        public CaseHelper()
        {
            _blaiseCaseApi = new BlaiseCaseApi();
        }

        public static CaseHelper GetInstance()
        {
            return _currentInstance ?? (_currentInstance = new CaseHelper());
        }

        public void CreateCasesInBlaise(int expectedNumberOfCases, string instrumentName, string serverParkName, string outcomeCode = "0")
        {
            var primaryKey = 900000;
            for (var count = 0; count < expectedNumberOfCases; count++)
            {
                var caseModel = new CaseModel(primaryKey.ToString(), outcomeCode, ModeType.Tel, DateTime.Now.AddHours(-1));
                CreateCaseInBlaise(caseModel, instrumentName, serverParkName);
                primaryKey++;
            }
        }

        public void CreateCasesInFile(int expectedNumberOfCases, string databaseFile, string outcomeCode = "0")
        {
            var primaryKey = 900000;
            for (var count = 0; count < expectedNumberOfCases; count++)
            {
                var caseModel = new CaseModel(primaryKey.ToString(), outcomeCode, ModeType.Web, DateTime.Now.AddMinutes(-20));
                CreateCaseInFile(databaseFile, caseModel);
                primaryKey++;
            }
        }

        public void CreateCaseInBlaise(CaseModel caseModel, string instrumentName, string serverParkName)
        {
            var dataFields = BuildDataFieldsFromCaseModel(caseModel);

            _blaiseCaseApi.CreateCase(caseModel.PrimaryKey, dataFields, instrumentName, serverParkName);
        }

        public void CreateCaseInFile(string databaseFile, CaseModel caseModel)
        {
            var dataFields = BuildDataFieldsFromCaseModel(caseModel);

            _blaiseCaseApi.CreateCase(databaseFile, caseModel.PrimaryKey, dataFields);
        }

        private static Dictionary<string, string> BuildDataFieldsFromCaseModel(CaseModel caseModel)
        {
            return new Dictionary<string, string>
            {
                { FieldNameType.HOut.FullName(), caseModel.Outcome },
                { FieldNameType.Mode.FullName(), ((int)caseModel.Mode).ToString() },
                { FieldNameType.TelNo.FullName(), "07000000000" },
                { FieldNameType.LastUpdated.FullName(), caseModel.LastUpdated.ToString("dd-MM-yyyy HH:mm:ss") },
                { FieldNameType.LastUpdatedDate.FullName(), caseModel.LastUpdated.ToString("dd-MM-yyyy") },
                { FieldNameType.LastUpdatedTime.FullName(), caseModel.LastUpdated.ToString("HH:mm:ss") }
            };
        }

        public IEnumerable<CaseModel> GetCasesInDatabase(string instrumentName, string serverParkName)
        {
            var caseModels = new List<CaseModel>();

            var casesInDatabase = _blaiseCaseApi.GetCases(instrumentName, serverParkName);

            while (!casesInDatabase.EndOfSet)
            {
                var caseRecord = casesInDatabase.ActiveRecord;
                var outcome = _blaiseCaseApi.GetFieldValue(caseRecord, FieldNameType.HOut).IntegerValue.ToString(CultureInfo.InvariantCulture);
                var mode = _blaiseCaseApi.GetFieldValue(caseRecord, FieldNameType.Mode).EnumerationValue;

                caseModels.Add(new CaseModel(_blaiseCaseApi.GetPrimaryKeyValue(caseRecord), outcome, (ModeType)mode, DateTime.Now));
                casesInDatabase.MoveNext();
            }

            return caseModels;
        }

        public int NumberOfCasesInInstrument(string instrumentName, string serverParkName)
        {
            return _blaiseCaseApi.GetNumberOfCases(instrumentName, serverParkName);
        }

        public void DeleteCasesInBlaise(string instrumentName, string serverParkName)
        {
            _blaiseCaseApi.RemoveCases(instrumentName, serverParkName);
        }

        public bool CaseExists(string primaryKeyValue, string instrumentName, string serverParkName)
        {
            return _blaiseCaseApi.CaseExists(primaryKeyValue, instrumentName, serverParkName);
        }
    }
}
