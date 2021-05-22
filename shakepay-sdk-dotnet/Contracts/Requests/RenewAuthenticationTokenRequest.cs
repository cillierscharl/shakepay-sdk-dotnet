using Newtonsoft.Json;

namespace ShakePay.Contracts
{
    public class RenewAuthenticationTokenRequest
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("strategy")]
        public string Strategy { get; set; }
    }
}
