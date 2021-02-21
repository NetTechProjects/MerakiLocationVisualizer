using System;
using System.Text.Json.Serialization;

namespace MerakiLocationVisualizer
{
    public class Observation
    {
        [JsonPropertyName("ipv4")]
        public string Ipv4 { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }

        [JsonPropertyName("seenTime")]
        public DateTime SeenTime { get; set; }

        [JsonPropertyName("ssid")]
        public string Ssid { get; set; }

        [JsonPropertyName("os")]
        public string Os { get; set; }

        [JsonPropertyName("clientMac")]
        public string ClientMac { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("seenEpoch")]
        public int SeenEpoch { get; set; }

        [JsonPropertyName("rssi")]
        public int Rssi { get; set; }

        [JsonPropertyName("ipv6")]
        public object Ipv6 { get; set; }

        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; }
    }
}
