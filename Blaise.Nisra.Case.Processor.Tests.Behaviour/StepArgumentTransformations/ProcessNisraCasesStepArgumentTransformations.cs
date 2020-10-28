using System.Collections.Generic;
using Blaise.Nisra.Case.Processor.Tests.Behaviour.Models;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Blaise.Nisra.Case.Processor.Tests.Behaviour.StepArgumentTransformations
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
