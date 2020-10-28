using System.Collections.Generic;
using Nisra.Case.Processor.Tests.Behaviour.Models;

namespace Nisra.Case.Processor.Tests.Behaviour.StepArgumentTransformations
{
    [Binding]
    public class ProcessNisraCasesStepArgumentTransformations
    {
        [StepArgumentTransformation]
        public IEnumerable<CaseModel> TransformCasesTableIntoListOfCaseModels(Table table)
        {
            return table.CreateSet<CaseModel>();
        }
    }
}
