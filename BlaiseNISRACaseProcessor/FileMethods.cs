using System.IO;

namespace BlaiseNisraCaseProcessor
{
    public static class FileMethods
    {
        public static bool CheckFileLock(string file)
        {
            FileInfo fileObj = new FileInfo(file);
            FileStream stream = null;
            try
            {
                stream = fileObj.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                // file locked
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            // file not locked
            return false;
        }
    }
}