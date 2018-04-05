using Newtonsoft.Json;

namespace CustomerRecognition.Common.Message
{
    public class IdentifyWaitingCustomersRequest:IdentifyCustomerRequest
    {

    }

    public class IdentifyWaitingCustomersResponse : BaseResponse
    {
        [JsonProperty("results")]
        public Identification[] Results { get; set; }
    }
}
