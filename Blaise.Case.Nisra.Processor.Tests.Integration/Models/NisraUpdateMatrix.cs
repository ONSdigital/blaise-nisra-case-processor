
using System;

namespace Blaise.Case.Nisra.Processor.Tests.Integration.Models
{
    public class NisraMatrixModel
    {
        public string PrimaryKeyValue { get; set; }

        public int NisraOutcomeCode { get; set; }

        public DateTime? NisraLastUpdatedDateTime { get; set; }

        public int? ExistingOutcomeCode { get; set; }

        public DateTime? ExistingLastUpdatedDateTime { get; set; }
    }
}
