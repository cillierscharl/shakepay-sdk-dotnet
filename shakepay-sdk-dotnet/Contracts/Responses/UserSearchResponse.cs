using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShakePay.Contracts
{
    public class UserSearchResponse
    {
        [JsonProperty("data")]
        public IList<UserSearchObject> Users { get; set; }
    }

    public class UserSearchObject
    {
        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
