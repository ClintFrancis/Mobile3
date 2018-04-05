using System;
using CustomerRecognition.Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CustomerRecognition.Common
{
    public class Order
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [JsonProperty("orderNumber")]
        public int OrderNumber { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("total")]
        public double Total { get; set; }

        [JsonProperty("emotion")]
        public string Emotion { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public OrderStatus Status { get; set; }

        [JsonProperty("customerImageUrl")]
        public string CustomerImageUrl { get; set; }

        public override string ToString()
        {
            return string.Format("[Order: id={0}, CustomerId={1}, OrderNumber={2}, Date={3}, Description={4}, Total={5}, Emotion={6}, Status={7}, CustomerImageUrl={8}]", id, CustomerId, OrderNumber, Date, Description, Total, Emotion, Status, CustomerImageUrl);
        }
    }
}
