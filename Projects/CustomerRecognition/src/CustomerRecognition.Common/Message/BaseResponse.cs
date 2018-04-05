using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerRecognition.Common.Message
{
    public class BaseResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("errorCode")]
        public int ErrorCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public bool HasError
        {
            get { return (ErrorCode != 0) || (StatusCode != 0); }
        }
    }
}
