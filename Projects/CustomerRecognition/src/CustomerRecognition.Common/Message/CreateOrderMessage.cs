using Newtonsoft.Json;

namespace CustomerRecognition.Common.Message
{
    public class CreateOrderRequest
    {
        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("order")]
        public Order Order { get; set; }
    }

    public class CreateOrderResponse : BaseResponse
    {
        [JsonProperty("order")]
        public Order Order { get; set; }
    }
}
