using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShakePay.Contracts
{
    public class TransactionHistoryRequest
    {
        [JsonProperty("pagination")]
        public TransactionHistoryPagination Pagination { get; set; }
        [JsonProperty("filterParams")]
        public TransactionHistoryFilterParams FilterParams { get; set; }
    }

    public class TransactionHistoryFilterParams
    {
        [JsonProperty("currencies")]
        public List<string> Currencies { get; set; }
    }

    public class TransactionHistoryPagination
    {
        [JsonProperty("descending")]
        public bool Descending { get; set; }
        [JsonProperty("rowsPerPage")]
        public int RowsPerPage { get; set; }
        [JsonProperty("page")]
        public string Page { get; set; }
    }
}
