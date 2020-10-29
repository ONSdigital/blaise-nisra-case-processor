using System.Collections.Generic;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Models;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.StepArgumentTransformations
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
