using Newtonsoft.Json;

namespace Blaise.Case.Nisra.Processor.MessageBroker.Model
{
    public class MessageModel
    {
        [JsonProperty("server_park_name")]
        public string ServerParkName { get; set; }

        [JsonProperty("instrument_name")]
        public string InstrumentName { get; set; }

        [JsonProperty("bucket_path")]
        public string BucketPath { get; set; }
    }
}
