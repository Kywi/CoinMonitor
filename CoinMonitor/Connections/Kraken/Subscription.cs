using Newtonsoft.Json;

namespace CoinMonitor.Connections.Kraken
{
    public class Subscription
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}