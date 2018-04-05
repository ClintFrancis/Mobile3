using System;
using Newtonsoft.Json;

namespace CustomerRecognition.Common.Message
{
    public class CustomerDetailsRequest
    {
        [JsonProperty("customerId")]
        public string CustomerId { get; set; }
        [JsonProperty("orderHistoryTotal")]
        public int OrderHistoryTotal { get; set; }
    }

    public class CustomerDetailsResponse : BaseResponse
    {
        [JsonProperty("customer")]
        public Customer Customer { get; set; }
        [JsonProperty("orderHistory")]
        public Order[] OrderHistory { get; set; }
    }
}
