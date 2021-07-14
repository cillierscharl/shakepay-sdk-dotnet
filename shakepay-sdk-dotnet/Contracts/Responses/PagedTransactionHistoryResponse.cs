using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ShakePay.Contracts
{
    public class PagedTransactionHistoryResponse
    {
        [JsonProperty("data")]
        public List<Transaction> Transactions { get; set; }
    }

    public class Transaction
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
        [JsonProperty("direction")]
        public string Direction { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("to")]
        public To To { get; set; }
        [JsonProperty("from")]
        public From From { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Transaction);
        }

        public bool Equals(Transaction transaction)
        {
            return transaction != null &&
                transaction.TransactionId == TransactionId;
        }

        public override int GetHashCode()
        {
            return TransactionId.GetHashCode();
        }
    }

    public class To
    {
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class From
    {
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
