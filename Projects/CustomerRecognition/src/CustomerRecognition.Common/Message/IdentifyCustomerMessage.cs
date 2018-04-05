using Newtonsoft.Json;
using System;

namespace CustomerRecognition.Common.Message
{
    public class IdentifyCustomerRequest
    {
        [JsonProperty("image")]
        public string Image { get; set; }

        public byte[] ImageAsByteArray()
        {
            return Convert.FromBase64String(Image);
        }
    }

    public class IdentifyCustomerResponse : BaseResponse
    {
        [JsonProperty("customer")]
        public Customer Customer { get; set; }
        [JsonProperty("emotion")]
        public string Emotion { get; set; }
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
        [JsonProperty("previousOrder")]
        public Order PreviousOrder { get; set; }

        public bool IsExistingCustomer()
        {
            return Customer?.PersonId != null;
        }
    }
}
