using Newtonsoft.Json;

namespace ShakePay.Contracts
{
    public class TransactionRequest
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }
        [JsonProperty("fromWallet")]
        public string FromWallet { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
        [JsonProperty("to")]
        public string To { get; set; }
        [JsonProperty("toType")]
        public string ToType { get; set; }
    }
}
