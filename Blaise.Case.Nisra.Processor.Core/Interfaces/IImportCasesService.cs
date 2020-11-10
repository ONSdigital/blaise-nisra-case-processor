namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IImportCasesService
    {
        void ImportCasesFromFile(string databaseFile, string serverPark, string surveyName);
    }
}