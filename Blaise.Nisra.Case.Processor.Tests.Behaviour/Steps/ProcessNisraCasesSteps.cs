using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Blaise.Nisra.Case.Processor.Tests.Behaviour.Enums;
using Blaise.Nisra.Case.Processor.Tests.Behaviour.Helpers;
using Blaise.Nisra.Case.Processor.Tests.Behaviour.Models;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Blaise.Nisra.Case.Processor.Tests.Behaviour.Steps
{
    [Binding]
    public sealed class ProcessNisraCasesSteps
    {
        private readonly ScenarioContext _scenarioContext;

        private readonly NisraFileHelper _nisraFileHelper;
        private readonly CaseHelper _caseHelper;
        private readonly BucketHelper _bucketHelper;
        private readonly PubSubHelper _pubSubHelper;
        private readonly ConfigurationHelper _configurationHelper;

        public ProcessNisraCasesSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;

            _nisraFileHelper = new NisraFileHelper();
            _caseHelper = new CaseHelper();
            _bucketHelper = new BucketHelper();
            _pubSubHelper = new PubSubHelper();
            _configurationHelper = new ConfigurationHelper();
        }

        [Given(@"there is a not a Nisra file available")]
        public void GivenThereIsANotANisraFileAvailable()
        {
        }

        [Given(@"there is a Nisra file that contains '(.*)' cases")]
        public void GivenThereIsANisraFileAvailable(int numberOfCases)
        {
            var nisraFilePath = _nisraFileHelper.CreateDatabaseFilesAndFolder();

            _caseHelper.CreateCases(nisraFilePath, numberOfCases);
            UploadNisraFile(nisraFilePath);

            _scenarioContext.Set(nisraFilePath, "nisraFilePath");
        }

        [Given(@"blaise contains no cases")]
        public void GivenTheBlaiseDatabaseIsEmpty()
        {
            _caseHelper.DeleteCasesInDatabase();
        }

        [Given(@"there is a Nisra file that contains the following cases")]
        public void GivenTheNisraFileContainsCasesToProcess(IEnumerable<CaseModel> cases)
        {
            var nisraFilePath = _nisraFileHelper.CreateDatabaseFilesAndFolder();

            _caseHelper.CreateCases(nisraFilePath, cases);
            UploadNisraFile(nisraFilePath);

            _scenarioContext.Set(nisraFilePath, "nisraFilePath");
        }

        [Given(@"there is a Nisra file that contains a case with the outcome code '(.*)'")]
        public void GivenThereIsANisraFileThatContainsACaseWithTheOutcomeCode(int outcomeCode)
        {
            var nisraFilePath = _nisraFileHelper.CreateDatabaseFilesAndFolder();
            var primaryKey = _caseHelper.CreateCase(nisraFilePath, outcomeCode, ModeType.Web);
            UploadNisraFile(nisraFilePath);

            _scenarioContext.Set(nisraFilePath, "nisraFilePath");
            _scenarioContext.Set(primaryKey, "primaryKey");
        }

        [Given(@"there is a Nisra file that contains a case that is complete")]
        public void GivenThereIsANisraFileThatContainsACaseThatIsComplete()
        {
            GivenThereIsANisraFileThatContainsACaseWithTheOutcomeCode(110);
        }

        [Given(@"there is a Nisra file that contains a case that is partially complete")]
        public void GivenThereIsANisraFileThatContainsACaseThatIsPartiallyComplete()
        {
            GivenThereIsANisraFileThatContainsACaseWithTheOutcomeCode(210);
        }

        [Given(@"there is a Nisra file that contains a case that has not been started")]
        public void GivenThereIsANisraFileThatContainsACaseThatHasNotBeenStarted()
        {
            GivenThereIsANisraFileThatContainsACaseWithTheOutcomeCode(0);
        }

        [Given(@"the same case exists in Blaise with the outcome code '(.*)'")]
        public void GivenTheSameCaseExistsInBlaiseWithTheOutcomeCode(int outcomeCode)
        {
            var primaryKey = _scenarioContext.Get<int>("primaryKey");
            _caseHelper.CreateCaseInDatabase(primaryKey, outcomeCode, ModeType.Tel);
        }

        [Given(@"the same case exists in Blaise that is complete")]
        public void GivenTheSameCaseExistsInBlaiseThatIsComplete()
        {
            GivenTheSameCaseExistsInBlaiseWithTheOutcomeCode(110);
        }


        [Given(@"the same case exists in Blaise that is partially complete")]
        public void GivenTheSameCaseExistsInBlaiseThatIsPartiallyComplete()
        {
            GivenTheSameCaseExistsInBlaiseWithTheOutcomeCode(210);
        }



        [Given(@"blaise contains '(.*)' cases")]
        public void GivenBlaiseContainsCases(int numberOfCases)
        {
            _caseHelper.CreateCasesInDatabase(numberOfCases);
        }

        [Given(@"blaise contains the following cases")]
        public void GivenTheBlaiseDatabaseAlreadyContainsCases(IEnumerable<CaseModel> cases)
        {
            _caseHelper.DeleteCasesInDatabase();

            _caseHelper.CreateCasesInDatabase(cases);
        }


        [When(@"the nisra file is processed")]
        public void TriggerAndMonitorProcess()
        {
            _pubSubHelper.PublishMessage(@"{ ""action"": ""process""}");

            var counter = 0;
            while (!_bucketHelper.FilesHaveBeenProcessed(_configurationHelper.BucketName))
            {
                Thread.Sleep(5000);
                counter++;

                if(counter == 20) return;
            }
        }

        [When(@"the nisra file is triggered every '(.*)' minutes for '(.*)' hour\(s\)")]
        public void WhenTheNisraFileIsProcessedEveryMinutesForHours(int minutes, int hours)
        {
            var startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalHours < hours)
            {
                Console.WriteLine("Start process at" + DateTime.Now);
                TriggerAndMonitorProcess();

                Console.WriteLine("Sleep for " + minutes + " minutes");
                Thread.Sleep(minutes * 60 * 1000);
            }

            Console.WriteLine("Finished after " + hours + " hour(s)");
        }

        [Then(@"blaise will contain no cases")]
        public void ThenBlaiseWillContainNoCases()
        {
            ThenCasesWillBeImportedIntoBlaise(0);
        }
        
        [Then(@"blaise will contain '(.*)' cases")]
        public void ThenCasesWillBeImportedIntoBlaise(int numberOfCases)
        {
            var numberOfCasesInBlaise = _caseHelper.GetNumberOfCasesInDatabase();

            Assert.AreEqual(numberOfCases, numberOfCasesInBlaise);
        }

        [Then(@"blaise will contain the following cases")]
        public void ThenTheBlaiseDatabaseWillContainTheFollowingCases(IEnumerable<CaseModel> cases)
        {
            var numberOfCasesInDatabase = _caseHelper.GetNumberOfCasesInDatabase();
            var casesExpected = cases.ToList();

            if (casesExpected.Count != numberOfCasesInDatabase)
            {
                Assert.Fail($"Expected '{casesExpected.Count}' cases in the database, but {numberOfCasesInDatabase} cases were found");
            }

            var casesInDatabase = _caseHelper.GetCasesInDatabase();

            foreach (var caseModel in casesInDatabase)
            {
                var caseRecordExpected = casesExpected.FirstOrDefault(c => c.PrimaryKey == caseModel.PrimaryKey);

                if (caseRecordExpected == null)
                {
                    Assert.Fail($"Case {caseModel.PrimaryKey} was in the database but not found in expected cases");
                }

                Assert.AreEqual(caseRecordExpected.Outcome, caseModel.Outcome, $"expected an outcome of '{caseRecordExpected.Outcome}' for case '{caseModel.PrimaryKey}'," +
                                                                               $"but was '{caseModel.Outcome}'");

                Assert.AreEqual(caseRecordExpected.Mode, caseModel.Mode, $"expected an version of '{caseRecordExpected.Mode}' for case '{caseModel.PrimaryKey}'," +
                                                                               $"but was '{caseModel.Mode}'");

            }
        }

        [Then(@"the existing blaise case is overwritten with the NISRA case")]
        public void ThenTheBlaiseCaseIsOverwrittenByTheNisraCase()
        {
            var primaryKey = _scenarioContext.Get<int>("primaryKey");
            var blaiseCase = _caseHelper.GetCaseInDatabase(primaryKey);
            
            Assert.AreEqual(ModeType.Web, blaiseCase.Mode);
        }

        [Then(@"the existing blaise case is kept")]
        public void ThenTheBlaiseCaseIsKept()
        {
            var primaryKey = _scenarioContext.Get<int>("primaryKey");
            var blaiseCase = _caseHelper.GetCaseInDatabase(primaryKey);

            Assert.AreEqual(ModeType.Tel, blaiseCase.Mode);
        }
        
        [AfterScenario]
        public static void CleanUpFiles()
        {
            var nisraFileHelper = new NisraFileHelper();
            nisraFileHelper.DeleteDatabaseFilesAndFolder();

            var caseHelper = new CaseHelper();
            caseHelper.DeleteCasesInDatabase();
        }

        private void UploadNisraFile(string nisraFilePath)
        {
            var databaseFilePath = Path.GetDirectoryName(nisraFilePath);

            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new Exception("The path to the database files does not exist");
            }

            foreach (var file in Directory.GetFiles(databaseFilePath))
            {
                _bucketHelper.UploadToBucket(file, _configurationHelper.BucketName);
            }
        }
    }
}
