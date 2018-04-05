using Newtonsoft.Json;

namespace CustomerRecognition.Common.Message
{
    public class ResetRequest:BaseResponse
    {
        [JsonProperty("customers")]
        public bool Customers { get; set; }

        [JsonProperty("orders")]
        public bool Orders { get; set; }

        [JsonProperty("faceGroups")]
        public bool FaceGroups { get; set; }

        [JsonProperty("images")]
        public bool Images { get; set; }
    }

    public class ResetResponse : BaseResponse
    {

    }
}
