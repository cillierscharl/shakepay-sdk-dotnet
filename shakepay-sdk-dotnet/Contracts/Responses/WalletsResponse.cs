using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShakePay.Contracts
{
    public class WalletsResponse
    {
        [JsonProperty("data")]
        public List<Wallet> Wallets { get; set; }
    }

    public class Wallet
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("balance")]
        public decimal Balance { get; set; }
        [JsonProperty("fiatBalance")]
        public decimal FiatBalance { get; set; }
    }
}
