using Newtonsoft.Json;

namespace ShakePay.Contracts
{
    public class ShakingSatsResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("className")]
        public string ClassName { get; set; }
        [JsonProperty("data")]
        public ShakeSatsResponseMetadata Data { get; set; }
        // Unknown
        [JsonProperty("errors")]
        public dynamic Errors { get; set; }
    }

    public class ShakeSatsResponseMetadata
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("streak")]
        public int Streak { get; set; }
    }
}
