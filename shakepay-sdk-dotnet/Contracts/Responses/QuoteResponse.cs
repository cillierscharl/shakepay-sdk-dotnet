using Newtonsoft.Json;

namespace ShakePay.Contracts
{
    public class QuoteResponse
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("baseRate")]
        public float BaseRate { get; set; }
        [JsonProperty("rate")]
        public float Rate { get; set; }
    }
}
