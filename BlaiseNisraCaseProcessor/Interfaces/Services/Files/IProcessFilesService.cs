using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Services.Files
{
    public interface IProcessFilesService
    {
        void ProcessFiles(IEnumerable<string> filesToProcess);
    }
}