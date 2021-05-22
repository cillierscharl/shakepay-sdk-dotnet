using Newtonsoft.Json;

namespace ShakePay.Contracts
{
    public class RenewAuthenticationTokenResponse
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
    }
}
