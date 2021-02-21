using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MerakiLocationVisualizer
{
    public class Data
    {
        [JsonPropertyName("apMac")]
        public string ApMac { get; set; }

        [JsonPropertyName("apFloors")]
        public List<object> ApFloors { get; set; }

        [JsonPropertyName("apTags")]
        public List<string> ApTags { get; set; }

        [JsonPropertyName("observations")]
        public List<Observation> Observations { get; set; }
    }
}
