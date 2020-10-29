using System.Collections.Generic;

namespace Blaise.Case.Nisra.Processor.Interfaces.Services
{
    public interface IProcessFilesService
    {
        void ProcessFiles(IList<string> filesToProcess);
    }
}