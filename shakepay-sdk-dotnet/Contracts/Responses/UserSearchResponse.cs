using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShakePay.Contracts
{
    public class UserSearchResponse
    {
        [JsonProperty("data")]
        public IList<Data> Users { get; set; }
    }

    public class Data
    {
        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
