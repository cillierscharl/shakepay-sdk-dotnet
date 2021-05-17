﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShakePay.Contracts
{
    public class WalletsResponse
    {
        [JsonProperty("data")]
        public IList<Data> Datas { get; set; }
    }

    public class Data
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("balance")]
        public int Balance { get; set; }
        [JsonProperty("fiatBalance")]
        public int FiatBalance { get; set; }
    }
}
