namespace Blaise.Nisra.Case.Processor.Interfaces.Services
{
    public interface IImportCasesService
    {
        void ImportCasesFromFile(string databaseFile, string serverPark, string surveyName);
    }
}