namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IImportFileService
    {
        void ImportSurveyRecordsFromFile(string databaseFile, string serverPark, string surveyName);
    }
}