using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BlaiseNisraCaseProcessor.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionType
    {
        NotSupported = 0,

        [EnumMember(Value = "process")]
        Process
    }
}
