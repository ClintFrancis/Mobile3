using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CustomerRecognition.Common
{
    public class Identification
    {
        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("emotion")]
        public string Emotion { get; set; }

        [JsonProperty("rectangle")]
        public Rectangle Rectangle { get; set; }

        [JsonProperty("orders")]
        public Order[] Orders { get; set; }
    }
}
