using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Blaise.Case.Nisra.Processor.Tests.Behaviour.Models;
using Blaise.Nuget.Api.Contracts.Enums;
using Blaise.Nuget.Api.Contracts.Extensions;
using Newtonsoft.Json;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Builders
{
    public static class CaseBuilder
    {
        public static Dictionary<string, string> BuildCaseData(this CaseModel caseModel)
        {
            var caseData = BuildDefaultCaseData();

            return AddCaseModelData(caseData, caseModel);
        }

        public static Dictionary<string, string> BuildBasicData(this CaseModel caseModel)
        {
            var caseData = new Dictionary<string, string>();
            return AddCaseModelData(caseData, caseModel);
        }

        private static Dictionary<string, string> BuildDefaultCaseData()
        {
            var dataFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Data");
            var caseData = GetCaseData(dataFolder);
            var fedForwardData = GetFedForwardData(dataFolder);

            //copy all entries in fed forward to case
            fedForwardData.ToList().ForEach(x => caseData.Add(x.Key, x.Value));

            return caseData;
        }

        private static Dictionary<string, string> AddCaseModelData(Dictionary<string, string> caseData, CaseModel caseModel)
        {
            caseData[FieldNameType.HOut.FullName()] = caseModel.Outcome;
            caseData["serial_number"] = caseModel.PrimaryKey;
            caseData[FieldNameType.Mode.FullName()] = ((int)caseModel.Mode).ToString();

            return caseData;
        }

        private static Dictionary<string, string> GetCaseData(string dataFolder)
        {
            var jsonCaseDataFilePath = Path.Combine(dataFolder, "CaseData.json");

            using (var file = File.OpenText(jsonCaseDataFilePath))
            {
                var serializer = new JsonSerializer();
                var caseData = (Dictionary<string, string>)serializer.Deserialize(file, typeof(Dictionary<string, string>));

                return caseData;
            }
        }

        private static Dictionary<string, string> GetFedForwardData(string dataFolder)
        {
            var jsonFedForwardDataFilePath = Path.Combine(dataFolder, "FedForwardData.json");

            using (var file = File.OpenText(jsonFedForwardDataFilePath))
            {
                var serializer = new JsonSerializer();
                var caseData = (Dictionary<string, string>)serializer.Deserialize(file, typeof(Dictionary<string, string>));

                return caseData;
            }
        }
    }
}
