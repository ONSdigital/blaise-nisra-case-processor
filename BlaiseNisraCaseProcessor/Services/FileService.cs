using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using BlaiseNisraCaseProcessor.Helpers;
using BlaiseNisraCaseProcessor.Interfaces.Services;

namespace BlaiseNisraCaseProcessor.Services
{
    public class FileService : IFileService
    {
        private const string DatabaseFileNameExt = ".bdix";
        private readonly IFileSystem _fileSystem;

        public FileService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public List<string> GetDatabaseFilesAvailable(IEnumerable<string> files)
        {
            return files.Where(f => f.ToLower().Contains(DatabaseFileNameExt)).ToList();
        }

        public string GetSurveyNameFromFile(string databaseFile)
        {
            return _fileSystem.Path.GetFileNameWithoutExtension(databaseFile);
        }
    }
}
