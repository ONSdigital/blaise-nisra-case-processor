namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IImportNisraDataFileService
    {
        void ImportNisraDatabaseFile(string serverParkName, string instrumentName, string databaseFilePath);
    }
}