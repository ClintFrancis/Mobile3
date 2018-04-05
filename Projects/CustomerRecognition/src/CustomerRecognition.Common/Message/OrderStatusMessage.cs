using Newtonsoft.Json;

namespace CustomerRecognition.Common.Message
{
    public class OrderStatusRequest
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("status")]
        public OrderStatus Status { get; set; }
    }

    public class OrderStatusResponse : BaseResponse
    {

    }
}
