using System.Collections.Generic;

namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IProcessNisraFilesService
    {
        void ProcessFiles(IList<string> filesToProcess);
    }
}