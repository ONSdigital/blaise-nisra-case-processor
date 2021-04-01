using System.IO;
using Blaise.Case.Nisra.Processor.Tests.Integration.Extensions;

namespace Blaise.Case.Nisra.Processor.Tests.Integration.Helpers
{
    public class NisraFileHelper
    {
        private static NisraFileHelper _currentInstance;

        public static NisraFileHelper GetInstance()
        {
            return _currentInstance ?? (_currentInstance = new NisraFileHelper());
        }

        public void CreateCasesInInstrumentFile(int numberOfCases, string instrumentName, string instrumentZipPath, string tempExtractPath, string outcomeCode)
        {
            var extractedFilePath = ExtractPackageFiles(tempExtractPath, instrumentName, instrumentZipPath);
            var instrumentDatabase = Path.Combine(extractedFilePath, $"{instrumentName}.bdix");

            CaseHelper.GetInstance().CreateCasesInFile(numberOfCases, instrumentDatabase, outcomeCode);
        }

        private static string ExtractPackageFiles(string path, string instrumentName, string instrumentPackage)
        {
            var extractedFilePath = Path.Combine(path, instrumentName);

            instrumentPackage.ExtractFiles(extractedFilePath);

            return extractedFilePath;
        }
    }
}
