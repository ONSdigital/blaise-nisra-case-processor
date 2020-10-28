using System.IO;
using System.Reflection;

namespace Blaise.Nisra.Case.Processor.Tests.Behaviour.Helpers
{
    public class NisraFileHelper
    {
        private readonly string _libraryFolder;
        private readonly string _localProcessFolder;
        private readonly string _databaseFileName;


        public NisraFileHelper()
        {
            var configurationHelper = new ConfigurationHelper();

            _libraryFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                        "LibraryFiles");
            
            _localProcessFolder = configurationHelper.LocalProcessFolder;
            _databaseFileName = $"{configurationHelper.InstrumentName}.{configurationHelper.DatabaseFileNameExt}";
        }

        public string CreateDatabaseFilesAndFolder()
        {
            if (Directory.Exists(_localProcessFolder))
            {
                DeleteDatabaseFilesAndFolder();
            }

            Directory.CreateDirectory(_localProcessFolder);

            CopyDatabaseLibraryFiles();

            return Path.Combine(_localProcessFolder, _databaseFileName);
        }

        public void DeleteDatabaseFilesAndFolder()
        {
            var directoryInfo = new DirectoryInfo(_localProcessFolder);

            foreach (var file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (var dir in directoryInfo.GetDirectories())
            {
                dir.Delete(true);
            }

            Directory.Delete(_localProcessFolder);
        }

        private void CopyDatabaseLibraryFiles()
        {
            foreach (var dirPath in Directory.GetDirectories(_libraryFolder, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(_libraryFolder, _localProcessFolder));

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(_libraryFolder, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(_libraryFolder, _localProcessFolder), true);
        }
    }
}
