using System;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Enums;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Models
{
    public class CaseModel
    {
        public CaseModel(string primaryKey, string outcome, ModeType mode, DateTime lastUpdated)
        {
            PrimaryKey = primaryKey;
            Outcome = outcome;
            Mode = mode;
            LastUpdated = lastUpdated;
        }

        public string PrimaryKey { get; set; }

        public string Outcome { get; set; }

        public ModeType Mode { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
