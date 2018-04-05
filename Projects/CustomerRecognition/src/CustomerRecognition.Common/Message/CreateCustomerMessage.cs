using Newtonsoft.Json;

namespace CustomerRecognition.Common.Message
{
    public class CreateCustomerRequest
    {
        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class CreateCustomerResponse : BaseResponse
    {
        public Customer Customer { get; set; }
    }
}
