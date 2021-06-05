using Newtonsoft.Json;

namespace ShakePay.Contracts
{
    public class RecentContact
    {
        [JsonProperty("matchId")]
        public string MatchId { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
