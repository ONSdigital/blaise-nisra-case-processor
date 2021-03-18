namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IImportNisraDataService
    {
        void ImportNisraDatabaseFile(string serverParkName, string instrumentName, string databaseFilePath);
    }
}