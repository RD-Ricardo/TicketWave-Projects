using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessPayment.Events
{
    public class RabbitMQEvent
    {
        [JsonProperty("eventSource")]
        public string EventSource { get; set; }

        [JsonProperty("eventSourceArn")]
        public string EventSourceArn { get; set; }

        [JsonProperty("rmqMessagesByQueue")]
        public Dictionary<string, List<RabbitMQMessage>> RmqMessagesByQueue { get; set; }
    }

    public class RabbitMQMessage
    {
        [JsonProperty("basicProperties")]
        public BasicProperties BasicProperties { get; set; }

        [JsonProperty("redelivered")]
        public bool Redelivered { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }

    public class BasicProperties
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("contentEncoding")]
        public string ContentEncoding { get; set; }

        [JsonProperty("headers")]
        public Dictionary<string, object> Headers { get; set; }

        [JsonProperty("deliveryMode")]
        public int? DeliveryMode { get; set; }

        [JsonProperty("priority")]
        public int? Priority { get; set; }

        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("replyTo")]
        public string ReplyTo { get; set; }

        [JsonProperty("expiration")]
        public string Expiration { get; set; }

        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("appId")]
        public string AppId { get; set; }

        [JsonProperty("clusterId")]
        public string ClusterId { get; set; }

        [JsonProperty("bodySize")]
        public int? BodySize { get; set; }
    }
}
