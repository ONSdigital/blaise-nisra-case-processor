namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IImportCasesService
    {
        void ImportCasesFromFile(string databaseFile, string serverPark, string surveyName);
    }
}