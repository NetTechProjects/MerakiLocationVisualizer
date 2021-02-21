using System.Text.Json.Serialization;

namespace MerakiLocationVisualizer
{
    public class DevicesSeen
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("secret")]
        public string Secret { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }

    }

}
