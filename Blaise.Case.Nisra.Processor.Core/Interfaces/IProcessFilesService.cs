using System.Collections.Generic;

namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IProcessFilesService
    {
        void ProcessFiles(IList<string> filesToProcess);
    }
}