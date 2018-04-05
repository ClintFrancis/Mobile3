using Newtonsoft.Json;

namespace CustomerRecognition.Common.Message
{
    
    public class OrdersRequest
    {
    }

    public class OrdersResponse : BaseResponse
    {
        [JsonProperty("orders")]
        public Order[] Orders { get; set; }
    }
}
