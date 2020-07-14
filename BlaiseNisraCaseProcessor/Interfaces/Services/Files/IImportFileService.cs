namespace BlaiseNisraCaseProcessor.Interfaces.Services.Files
{
    public interface IImportFileService
    {
        void ImportSurveyRecordsFromFile(string databaseFile, string serverPark, string surveyName);
    }
}