using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShakePay.Contracts
{
    public class TransactionHistoryRequest
    {
        [JsonProperty("pagination")]
        public TransactionHistoryPagination Pagination { get; set; }
        [JsonProperty("filterParams")]
        public dynamic FilterParams { get; set; }
    }

    public class TransactionHistoryPagination
    {
        [JsonProperty("descending")]
        public bool Descending { get; set; }
        [JsonProperty("rowsPerPage")]
        public int RowsPerPage { get; set; }
        [JsonProperty("page")]
        public int Page { get; set; }
    }
}
