using Newtonsoft.Json;

namespace ShakePay.Contracts
{
    public class QuoteResponse
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("baseRate")]
        public decimal BaseRate { get; set; }
        [JsonProperty("rate")]
        public decimal Rate { get; set; }
    }
}
