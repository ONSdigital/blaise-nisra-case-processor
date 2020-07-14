using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IProcessFilesService
    {
        void ProcessFiles(IEnumerable<string> filesToProcess);
    }
}