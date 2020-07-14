using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using BlaiseNisraCaseProcessor.Helpers;
using BlaiseNisraCaseProcessor.Interfaces.Services.Files;

namespace BlaiseNisraCaseProcessor.Services.Files
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

        public IEnumerable<string> GetFiles(string path, string filePattern)
        {
            path.ThrowExceptionIfNullOrEmpty("path");
            filePattern.ThrowExceptionIfNullOrEmpty("filePattern");

            var directory = GetDirectory(path);
            var files = directory.GetFiles(filePattern);

            return files.Select(f => f.FullName);
        }

        public string GetSurveyNameFromFile(string databaseFile)
        {
            return _fileSystem.Path.GetFileNameWithoutExtension(databaseFile);
        }

        private IDirectoryInfo GetDirectory(string path)
        {
            var directory = _fileSystem.DirectoryInfo.FromDirectoryName(path);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"The directory '{path}' was not found");
            }

            return directory;
        }
    }
}
