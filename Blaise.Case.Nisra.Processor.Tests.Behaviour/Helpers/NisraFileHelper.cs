using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Enums;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Extensions;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Models;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Helpers
{
    public class NisraFileHelper
    {
        private static NisraFileHelper _currentInstance;

        public static NisraFileHelper GetInstance()
        {
            return _currentInstance ?? (_currentInstance = new NisraFileHelper());
        }

        public async Task CreateCasesInOnlineFileAsync(int numberOfCases, string path)
        {
            var instrumentPackage = await DownloadPackageFromBucket(path);
            var extractedFilePath = ExtractPackageFiles(path, instrumentPackage);
            var instrumentDatabase = Path.Combine(extractedFilePath, BlaiseConfigurationHelper.InstrumentName + ".bdix");

            CaseHelper.GetInstance().CreateCasesInFile(instrumentDatabase, numberOfCases);

            await UploadFilesToBucket(extractedFilePath);
        }

        public async Task CreateCasesInOnlineFileAsync(IEnumerable<CaseModel> caseModels, string path)
        {
            var instrumentPackage = await DownloadPackageFromBucket(path);
            var extractedFilePath = ExtractPackageFiles(path, instrumentPackage);
            var instrumentDatabase = Path.Combine(extractedFilePath, BlaiseConfigurationHelper.InstrumentName + ".bdix");

            CaseHelper.GetInstance().CreateCasesInFile(instrumentDatabase, caseModels.ToList());

            await UploadFilesToBucket(extractedFilePath);
        }

        public async Task<string> CreateCaseInOnlineFileAsync(int outcomeCode, string path)
        {
            var instrumentPackage = await DownloadPackageFromBucket(path);
            var extractedFilePath = ExtractPackageFiles(path, instrumentPackage);
            var instrumentDatabase = Path.Combine(extractedFilePath, BlaiseConfigurationHelper.InstrumentName + ".bdix");

            var caseModel = CaseHelper.GetInstance().CreateCaseModel(outcomeCode.ToString(), ModeType.Web, DateTime.Now.AddMinutes(-40));
           CaseHelper.GetInstance().CreateCaseInFile(instrumentDatabase, caseModel);

            await UploadFilesToBucket(extractedFilePath);

            return caseModel.PrimaryKey;
        }

        public async Task CleanUpOnlineFiles()
        {
            await CloudStorageHelper.GetInstance().DeleteFilesInBucketAsync(BlaiseConfigurationHelper.NisraBucket,
                BlaiseConfigurationHelper.InstrumentName);
        }

        private static async Task<string> DownloadPackageFromBucket(string path)
        {
            return await CloudStorageHelper.GetInstance().DownloadFromBucketAsync(
                BlaiseConfigurationHelper.InstrumentPackageBucket,
                BlaiseConfigurationHelper.InstrumentFile, path);
        }

        private static string ExtractPackageFiles(string path, string instrumentPackage)
        {
            var extractedFilePath = Path.Combine(path, BlaiseConfigurationHelper.InstrumentName);

            instrumentPackage.ExtractFiles(extractedFilePath);

            return extractedFilePath;
        }

        private async Task UploadFilesToBucket(string filePath)
        {
            var uploadPath = Path.Combine(BlaiseConfigurationHelper.NisraBucket);

            await CloudStorageHelper.GetInstance().UploadFolderToBucketAsync(
                uploadPath, filePath);
        }
    }
}
