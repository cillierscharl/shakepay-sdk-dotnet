using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ShakePay.Contracts
{
    public class WaitlistResponse
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("badges")]
        public List<WaitListBadges> Badges { get; set; }
        [JsonProperty("history")]
        public List<WaitListHistory> Histories { get; set; }
    }

    public class WaitListBadges
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
        [JsonProperty("minRecipients")]
        public int MinRecipients { get; set; }
    }

    public class WaitListHistory
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }
        [JsonProperty("metadata")]
        public WaitListHistoryMetadata Metadata { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class WaitListHistoryMetadata
    {
        [JsonProperty("recipientId")]
        public string RecipientId { get; set; }
        [JsonProperty("streak")]
        public int Streak { get; set; }
    }
}
