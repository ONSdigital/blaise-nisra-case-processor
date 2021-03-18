using System;
using System.IO;
using System.Threading;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Helpers
{
    public class FileSystemHelper
    {
        private static FileSystemHelper _currentInstance;

        public static FileSystemHelper GetInstance()
        {
            return _currentInstance ?? (_currentInstance = new FileSystemHelper());
        }

        public void CleanUpTempFiles(string path)
        {
            if (!Directory.Exists(path)) return;

            var directoryInfo = new DirectoryInfo(path);
            var parentDirectory = directoryInfo.Parent?.Name;
            
            if (parentDirectory == null)
            {
                CleanUpFiles(path);
            }

            if (Guid.TryParse(Path.GetDirectoryName(parentDirectory), out _))
            {
                CleanUpFiles(parentDirectory);
                return;
            }

            CleanUpFiles(path);
        }

        private static void CleanUpFiles(string path)
        {
            try
            {
                Thread.Sleep(5000);
                DeleteDirectoryAndFilesInPath(path);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not cleanup folders, {e.Message}, {e}");
            }
        }

        private static void DeleteDirectoryAndFilesInPath(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            foreach (var dir in dirInfo.GetDirectories())
            {
                foreach (var file in dir.GetFiles())
                {
                    file.Delete();
                }

                dir.Delete(true);
            }

            foreach (var file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }

            Directory.Delete(path, true);
        }
    }
}