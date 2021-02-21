using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MerakiLocationVisualizer
{
    public class Location
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }

        [JsonPropertyName("unc")]
        public double Unc { get; set; }

        [JsonPropertyName("x")]
        public List<object> X { get; set; }

        [JsonPropertyName("y")]
        public List<object> Y { get; set; }
    }
}
