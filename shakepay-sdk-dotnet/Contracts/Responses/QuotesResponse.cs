using Newtonsoft.Json;

namespace ShakePay.Contracts
{
    class QuotesResponse
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("baseRate")]
        public float BaseRate { get; set; }
        [JsonProperty("rate")]
        public float Rate { get; set; }
    }
}
