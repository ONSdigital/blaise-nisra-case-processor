using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IFileService
    {
        List<string> GetDatabaseFilesAvailable(IEnumerable<string> files);
        IEnumerable<string> GetFiles(string path, string filePattern);
        string GetSurveyNameFromFile(string databaseFile);
    }
}