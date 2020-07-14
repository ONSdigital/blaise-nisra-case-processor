using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IFileService
    {
        List<string> GetDatabaseFilesAvailable(IEnumerable<string> files);
        string GetSurveyNameFromFile(string databaseFile);
    }
}