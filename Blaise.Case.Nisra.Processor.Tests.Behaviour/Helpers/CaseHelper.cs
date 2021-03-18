using System;
using System.Collections.Generic;
using System.Globalization;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Enums;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Models;
using Blaise.Nuget.Api.Api;
using Blaise.Nuget.Api.Contracts.Enums;
using Blaise.Nuget.Api.Contracts.Extensions;
using Blaise.Nuget.Api.Contracts.Interfaces;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Helpers
{
    public class CaseHelper
    {
        private readonly IBlaiseCaseApi _blaiseCaseApi;
        private int _primaryKey;

        private static CaseHelper _currentInstance;

        public CaseHelper()
        {
            _blaiseCaseApi = new BlaiseCaseApi();
            _primaryKey = 900000;
        }

        public static CaseHelper GetInstance()
        {
            return _currentInstance ?? (_currentInstance = new CaseHelper());
        }

        public CaseModel CreateCaseModel(string outCome, ModeType modeType, DateTime lastUpdated)
        {
            return new CaseModel(_primaryKey.ToString(), outCome, modeType, lastUpdated);
        }

        public void CreateCasesInBlaise(int expectedNumberOfCases)
        {
            for (var count = 0; count < expectedNumberOfCases; count++)
            {
                var caseModel = new CaseModel(_primaryKey.ToString(), "110", ModeType.Tel, DateTime.Now.AddHours(-1));
                CreateCaseInBlaise(caseModel);
                _primaryKey++;
            }
        }

        public void CreateCasesInFile(string extractedFilePath, int expectedNumberOfCases)
        {
            for (var count = 0; count < expectedNumberOfCases; count++)
            {
                var caseModel = new CaseModel(_primaryKey.ToString(), "110", ModeType.Web, DateTime.Now.AddMinutes(-20));
                CreateCaseInFile(extractedFilePath, caseModel);
                _primaryKey++;
            }
        }

        public void CreateCasesInFile(string extractedFilePath, IList<CaseModel> caseModels)
        {
            foreach (var caseModel in caseModels)
            {
                caseModel.LastUpdated = DateTime.Now.AddHours(-2);

                CreateCaseInFile(extractedFilePath, caseModel);
            }
        }

        public void CreateCasesInBlaise(IEnumerable<CaseModel> caseModels)
        {
            foreach (var caseModel in caseModels)
            {
                CreateCaseInBlaise(caseModel);
            }
        }

        public void CreateCaseInBlaise(CaseModel caseModel)
        {
            var dataFields = BuildDataFieldsFromCaseModel(caseModel);

            _blaiseCaseApi.CreateCase(caseModel.PrimaryKey, dataFields,
                BlaiseConfigurationHelper.InstrumentName, BlaiseConfigurationHelper.ServerParkName);
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
                { "SerialNumber", caseModel.PrimaryKey },
                { FieldNameType.HOut.FullName(), caseModel.Outcome },
                { FieldNameType.Mode.FullName(), ((int)caseModel.Mode).ToString() },
                { FieldNameType.LastUpdatedDate.FullName(), caseModel.LastUpdated.ToString("dd-MM-yyyy") },
                { FieldNameType.LastUpdatedTime.FullName(), caseModel.LastUpdated.ToString("HH:mm:ss") }
            };
        }

        public IEnumerable<CaseModel> GetCasesInDatabase()
        {
            var caseModels = new List<CaseModel>();

            var casesInDatabase = _blaiseCaseApi.GetCases(
                BlaiseConfigurationHelper.InstrumentName, BlaiseConfigurationHelper.ServerParkName);

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

        public void DeleteCases()
        {
            var cases = _blaiseCaseApi.GetCases(BlaiseConfigurationHelper.InstrumentName,
                BlaiseConfigurationHelper.ServerParkName);

            while (!cases.EndOfSet)
            {
                var primaryKey = _blaiseCaseApi.GetPrimaryKeyValue(cases.ActiveRecord);

                _blaiseCaseApi.RemoveCase(primaryKey, BlaiseConfigurationHelper.InstrumentName,
                    BlaiseConfigurationHelper.ServerParkName);

                cases.MoveNext();
            }
        }

        public int NumberOfCasesInInstrument()
        {
            return _blaiseCaseApi.GetNumberOfCases(BlaiseConfigurationHelper.InstrumentName,
                BlaiseConfigurationHelper.ServerParkName);
        }

        public ModeType GetMode(string primaryKey)
        {
            var field = _blaiseCaseApi.GetFieldValue(primaryKey, BlaiseConfigurationHelper.InstrumentName,
                BlaiseConfigurationHelper.ServerParkName, FieldNameType.Mode);

            return (ModeType)field.EnumerationValue;
        }

        public void MarkCaseAsOpenInCati(string primaryKey)
        {
            var dataRecord = _blaiseCaseApi.GetCase(primaryKey, BlaiseConfigurationHelper.InstrumentName,
                BlaiseConfigurationHelper.ServerParkName);

            var fieldData = new Dictionary<string, string> {
                {FieldNameType.LastUpdatedDate.FullName(), DateTime.Now.ToString("dd-MM-yyyy")},
                {FieldNameType.LastUpdatedTime.FullName(), DateTime.Now.ToString("HH:mm:ss")}};

            _blaiseCaseApi.UpdateCase(dataRecord, fieldData, BlaiseConfigurationHelper.InstrumentName,
                BlaiseConfigurationHelper.ServerParkName);
        }
    }
}
