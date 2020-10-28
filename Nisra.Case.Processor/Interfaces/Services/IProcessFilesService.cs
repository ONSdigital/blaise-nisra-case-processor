using System.Collections.Generic;

namespace Nisra.Case.Processor.Interfaces.Services
{
    public interface IProcessFilesService
    {
        void ProcessFiles(IList<string> filesToProcess);
    }
}