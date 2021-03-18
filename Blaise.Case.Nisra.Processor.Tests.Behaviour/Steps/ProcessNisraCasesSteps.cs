using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Enums;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Helpers;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Models;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Steps
{
    [Binding]
    public sealed class ProcessNisraCasesSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private static string _tempFilePath;

        public ProcessNisraCasesSteps(ScenarioContext scenarioContext)
        {
            _tempFilePath = Path.Combine(BlaiseConfigurationHelper.TempPath, "Tests", Guid.NewGuid().ToString());
            _scenarioContext = scenarioContext;
        }

        [BeforeFeature("importdata")]
        public static void SetupUpFeature()
        {
            InstrumentHelper.GetInstance().InstallSurvey();
        }

        [Given(@"there is a not a Nisra file available")]
        public void GivenThereIsANotANisraFileAvailable()
        {
        }

        [Given(@"there is a Nisra file that contains '(.*)' cases")]
        public async Task GivenThereIsANisraFileAvailable(int numberOfCases)
        {
            await NisraFileHelper.GetInstance().CreateCasesInOnlineFileAsync(numberOfCases, _tempFilePath);
        }

        [Given(@"blaise contains no cases")]
        public void GivenTheBlaiseDatabaseIsEmpty()
        {
            CaseHelper.GetInstance().DeleteCases();
        }

        [Given(@"there is a Nisra file that contains the following cases")]
        public async Task GivenTheNisraFileContainsCasesToProcess(IEnumerable<CaseModel> cases)
        {
            await NisraFileHelper.GetInstance().CreateCasesInOnlineFileAsync(cases, _tempFilePath);
        }

        [Given(@"there is a Nisra file that contains a case with the outcome code '(.*)'")]
        public async Task GivenThereIsANisraFileThatContainsACaseWithTheOutcomeCode(int outcomeCode)
        {
            var primaryKey = await NisraFileHelper.GetInstance().CreateCaseInOnlineFileAsync(outcomeCode, _tempFilePath);
            _scenarioContext.Set(primaryKey,"primaryKey");
        }

        [Given(@"there is a Nisra file that contains a case that is complete")]
        public async Task GivenThereIsANisraFileThatContainsACaseThatIsComplete()
        {
            await GivenThereIsANisraFileThatContainsACaseWithTheOutcomeCode(110);
        }

        [Given(@"there is a Nisra file that contains a case that is partially complete")]
        public async Task GivenThereIsANisraFileThatContainsACaseThatIsPartiallyComplete()
        {
            await GivenThereIsANisraFileThatContainsACaseWithTheOutcomeCode(210);
        }

        [Given(@"there is a Nisra file that contains a case that has not been started")]
        public  async Task GivenThereIsANisraFileThatContainsACaseThatHasNotBeenStarted()
        {
            await GivenThereIsANisraFileThatContainsACaseWithTheOutcomeCode(0);
        }

        [Given(@"the same case exists in Blaise with the outcome code '(.*)'")]
        public void GivenTheSameCaseExistsInBlaiseWithTheOutcomeCode(int outcomeCode)
        {
            var primaryKey = _scenarioContext.Get<string>("primaryKey");
            var caseModel = new CaseModel(primaryKey, outcomeCode.ToString(), ModeType.Tel, DateTime.Now.AddHours(-2));
            CaseHelper.GetInstance().CreateCaseInBlaise(caseModel);
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
            CaseHelper.GetInstance().CreateCasesInBlaise(numberOfCases);
        }

        [Given(@"blaise contains the following cases")]
        public void GivenTheBlaiseDatabaseAlreadyContainsCases(IEnumerable<CaseModel> cases)
        {
            CaseHelper.GetInstance().CreateCasesInBlaise(cases);
        }


        [When(@"the nisra file is processed")]
        public async Task TriggerAndMonitorProcess()
        {
            PubSubHelper.GetInstance().PublishMessage(
                $@"{{ ""ServerParkName"": ""{BlaiseConfigurationHelper.ServerParkName}"" , ""InstrumentName"": ""{BlaiseConfigurationHelper.InstrumentName}""}}");

            var counter = 0;
            while (!await CloudStorageHelper.GetInstance().FilesHaveBeenProcessedAsync(BlaiseConfigurationHelper.NisraBucket))
            {
                Thread.Sleep(5000);
                counter++;

                if (counter == 20) return;
            }
        }

        [When(@"the nisra file is triggered every '(.*)' minutes for '(.*)' hour\(s\)")]
        public async Task WhenTheNisraFileIsProcessedEveryMinutesForHours(int minutes, int hours)
        {
            var startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalHours < hours)
            {
                Console.WriteLine("Start process at" + DateTime.Now);
                await TriggerAndMonitorProcess();

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
            var numberOfCasesInBlaise = CaseHelper.GetInstance().NumberOfCasesInInstrument();

            Assert.AreEqual(numberOfCases, numberOfCasesInBlaise);
        }

        [Then(@"blaise will contain the following cases")]
        public void ThenTheBlaiseDatabaseWillContainTheFollowingCases(IEnumerable<CaseModel> cases)
        {
            var numberOfCasesInDatabase = CaseHelper.GetInstance().NumberOfCasesInInstrument();
            var casesExpected = cases.ToList();

            if (casesExpected.Count != numberOfCasesInDatabase)
            {
                Assert.Fail($"Expected '{casesExpected.Count}' cases in the database, but {numberOfCasesInDatabase} cases were found");
            }

            var casesInDatabase = CaseHelper.GetInstance().GetCasesInDatabase();

            foreach (var caseModel in casesInDatabase)
            {
                var caseRecordExpected = casesExpected.FirstOrDefault(c => c.PrimaryKey == caseModel.PrimaryKey);

                if (caseRecordExpected == null)
                {
                    Assert.Fail($"Case {caseModel.PrimaryKey} was in the database but not found in expected cases");
                }

                Assert.AreEqual(caseRecordExpected.Outcome, caseModel.Outcome,
                    $"expected an outcome of '{caseRecordExpected.Outcome}' for case '{caseModel.PrimaryKey}'," +
                    $"but was '{caseModel.Outcome}'");

                Assert.AreEqual(caseRecordExpected.Mode, caseModel.Mode,
                    $"expected an version of '{caseRecordExpected.Mode}' for case '{caseModel.PrimaryKey}'," +
                    $"but was '{caseModel.Mode}'");
            }
        }

        [Then(@"the existing blaise case is overwritten with the NISRA case")]
        public void ThenTheBlaiseCaseIsOverwrittenByTheNisraCase()
        {
            var primaryKey = _scenarioContext.Get<string>("primaryKey");
            var modeType = CaseHelper.GetInstance().GetMode(primaryKey);
            Assert.AreEqual(ModeType.Web, modeType);
        }

        [Then(@"the existing blaise case is kept")]
        public void ThenTheBlaiseCaseIsKept()
        {
            var primaryKey = _scenarioContext.Get<string>("primaryKey");
            var modeType = CaseHelper.GetInstance().GetMode(primaryKey);
            Assert.AreEqual(ModeType.Tel, modeType);
        }

        [Given(@"the case has been updated within the past 30 minutes")]
        public void GivenTheCaseIsCurrentlyOpenInCati()
        {
            var primaryKey = _scenarioContext.Get<string>("primaryKey");
            CaseHelper.GetInstance().MarkCaseAsOpenInCati(primaryKey);
        }

        [Then(@"the nisra case is not imported again")]
        public void ThenTheNisraCaseIsNotImportedAgain()
        {
            var primaryKey = _scenarioContext.Get<string>("primaryKey");
            var modeType = CaseHelper.GetInstance().GetMode(primaryKey);
            Assert.AreEqual(ModeType.Web, modeType);
        }

        [AfterScenario]
        public static async Task CleanUpScenario()
        {
            CaseHelper.GetInstance().DeleteCases();
            await NisraFileHelper.GetInstance().CleanUpOnlineFiles();
            FileSystemHelper.GetInstance().CleanUpTempFiles(_tempFilePath);
        }

        [AfterFeature("importdata")]
        public static void CleanUpFeature()
        {
            InstrumentHelper.GetInstance().UninstallSurvey();
        }
    }
}
