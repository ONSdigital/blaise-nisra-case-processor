namespace Blaise.Case.Nisra.Processor.Core.Interfaces
{
    public interface IProcessNisraCasesService
    {
        void ProcessNisraCases(string nisraDatabaseFile, string serverPark, string surveyName);
    }
}