using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure;

namespace WordTranslatorInc.Models
{
    public class Translation : ITableEntity
    {
        [JsonProperty(PropertyName = "userid")]
        string UserId { get; set; }

        [JsonProperty]
        string Word { get; set; }

        [JsonProperty]
        int Number { get; set; }

        [JsonIgnore]
        string OperationTimestamp { get; set; }

        public string PartitionKey { get => UserId; set => UserId = value; }

        public string RowKey { get => OperationTimestamp; set => OperationTimestamp = value; }

        public DateTimeOffset Timestamp { get => DateTimeOffset.UtcNow; set => Timestamp = value; }

        DateTimeOffset? ITableEntity.Timestamp { get => DateTimeOffset.UtcNow; set => Console.WriteLine("Do nothing"); }

        ETag ITableEntity.ETag { get => new ETag(); set => Console.WriteLine("Do nothing"); }

        public Translation(string userId, string word, int num)
        {
            UserId = userId;
            this.Word = word;
            Number = num;
            PartitionKey = userId;
            OperationTimestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            RowKey = OperationTimestamp;
        }

        public Translation()
        {}
    }
}
