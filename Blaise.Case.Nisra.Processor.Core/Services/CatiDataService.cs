using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Blaise.Case.Nisra.Processor.Core.Extensions;
using Blaise.Case.Nisra.Processor.Core.Interfaces;

namespace Blaise.Case.Nisra.Processor.Core.Services
{
    public class CatiDataService : ICatiDataService
    {
        public void RemoveCatiManaBlock(Dictionary<string, string> fieldData)
        {
            var callHistoryItems = fieldData.Where(f =>
                f.Key.StartsWith("CatiMana")).ToList();

            foreach (var callHistoryItem in callHistoryItems)
            {
                fieldData.Remove(callHistoryItem.Key);
            }
        }

        public void RemoveCallHistoryBlock(Dictionary<string, string> fieldData)
        {
            var callHistoryItems = fieldData.Where(f =>
                f.Key.StartsWith("CallHistory")).ToList();

            foreach (var callHistoryItem in callHistoryItems)
            {
                fieldData.Remove(callHistoryItem.Key);
            }
        }

        public void RemoveWebNudgedField(Dictionary<string, string> fieldData)
        {
            if (fieldData.ContainsKey("WebNudged"))
            {
                fieldData.Remove("WebNudged");
            }
        }

        public void AddCatiManaCallItems(Dictionary<string, string> newFieldData,
            Dictionary<string, string> existingFieldData, int outcomeCode)
        {
            var catiCallItems = BuildCatiManaRegCallItems(existingFieldData,
                outcomeCode);

            AddCatiManaNrOfCallItem(newFieldData, existingFieldData);
            SetFirstDayIfNotSet(newFieldData, existingFieldData);

            foreach (var catiCallItem in catiCallItems)
            {
                newFieldData.Add(catiCallItem.Key, catiCallItem.Value);
            }
        }

        public void SetFirstDayIfNotSet(Dictionary<string, string> newFieldData,
            Dictionary<string, string> existingFieldData)
        {
            var existingFieldValue = existingFieldData["CatiMana.CatiCall.FirstDay"];

            newFieldData.Add("CatiMana.CatiCall.FirstDay",
                string.IsNullOrWhiteSpace(existingFieldValue)
                    ? DateTime.Now.ToString("ddMMyyyy", CultureInfo.InvariantCulture)
                    : existingFieldValue);
        }

        public void AddCatiManaNrOfCallItem(Dictionary<string, string> newFieldData,
            Dictionary<string, string> existingFieldData)
        {
            newFieldData.Add("CatiMana.CatiCall.NrOfCall",
                int.TryParse(existingFieldData["CatiMana.CatiCall.NrOfCall"], out var numberOfCalls)
                    ? (numberOfCalls + 1).ToString()
                    : "1");
        }

        public Dictionary<string, string> BuildCatiManaRegCallItems(Dictionary<string, string> fieldData, int outcomeCode)
        {
            var newDictionary = new Dictionary<string, string>();
            newDictionary.AddRange(BuildOnlineCatiCallItems(outcomeCode, 1));
            newDictionary.AddRange(BuildTelCatiCallItems(fieldData, 2, 1));
            newDictionary.AddRange(BuildTelCatiCallItems(fieldData, 3, 2));
            newDictionary.AddRange(BuildTelCatiCallItems(fieldData, 4, 3));
            newDictionary.AddRange(string.IsNullOrWhiteSpace(fieldData["CatiMana.CatiCall.RegsCalls[5].WhoMade"])
                ? BuildOnlineCatiCallItems(outcomeCode, 5)
                : BuildTelCatiCallItems(fieldData, 5, 5));

            return newDictionary;
        }

        private static Dictionary<string, string> BuildOnlineCatiCallItems(int outcomeCode, int entryNumber)
        {
            return new Dictionary<string, string>
            {
                {$"CatiMana.CatiCall.RegsCalls[{entryNumber}].WhoMade", "Web"},
                {$"CatiMana.CatiCall.RegsCalls[{entryNumber}].DayNumber", "1"},
                {$"CatiMana.CatiCall.RegsCalls[{entryNumber}].DialTime", $"{DateTime.Now:HH:mm:ss}"},
                {$"CatiMana.CatiCall.RegsCalls[{entryNumber}].NrOfDials", "1"},
                {$"CatiMana.CatiCall.RegsCalls[{entryNumber}].DialResult", outcomeCode == 110 ? "1" : "2"}
            };
        }

        private static Dictionary<string, string> BuildTelCatiCallItems(IReadOnlyDictionary<string, string> fieldData, int newEntryNumber,
            int existingEntryNumber)
        {
            return new Dictionary<string, string>
            {
                {$"CatiMana.CatiCall.RegsCalls[{newEntryNumber}].WhoMade", fieldData[$"CatiMana.CatiCall.RegsCalls[{existingEntryNumber}].WhoMade"]},
                {$"CatiMana.CatiCall.RegsCalls[{newEntryNumber}].DayNumber", fieldData[$"CatiMana.CatiCall.RegsCalls[{existingEntryNumber}].DayNumber"]},
                {$"CatiMana.CatiCall.RegsCalls[{newEntryNumber}].DialTime", fieldData[$"CatiMana.CatiCall.RegsCalls[{existingEntryNumber}].DialTime"]},
                {$"CatiMana.CatiCall.RegsCalls[{newEntryNumber}].NrOfDials", fieldData[$"CatiMana.CatiCall.RegsCalls[{existingEntryNumber}].NrOfDials"]},
                {$"CatiMana.CatiCall.RegsCalls[{newEntryNumber}].DialResult", fieldData[$"CatiMana.CatiCall.RegsCalls[{existingEntryNumber}].DialResult"]},
            };
        }
    }
}
