using System;
using Newtonsoft.Json;

namespace CustomerRecognition.Common
{
    public class Customer
    {
        [JsonProperty("id")]
        public String id { get; set; }

        [JsonProperty("faceId")]
        public Guid FaceId { get; set; }

        [JsonProperty("personId")]
        public Guid PersonId { get; set; }

        [JsonProperty("firstName")]
        public String FirstName { get; set; }

        [JsonProperty("lastName")]
        public String LastName { get; set; }

        [JsonProperty("age")]
        public double Age { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("anonymous")]
        public bool Anonymous
        {
            get
            {
                Guid result;
                return Guid.TryParse(FirstName, out result);
            }
            set { }
        }

        public string RegistrationName()
        {
            Guid result;
            if (!Guid.TryParse(FirstName, out result))
            {
                var first = string.IsNullOrEmpty(FirstName) ? "" : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(FirstName);
                var last = string.IsNullOrEmpty(LastName) ? "" : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(LastName);
                return first + last;
            }

            return FirstName;
        }

        public override string ToString()
        {
            return string.Format("[Customer: id={0}, FaceId={1}, PersonId={2}, FirstName={3}, LastName={4}, Age={5}, Gender={6}, Anonymous={7}]", id, FaceId, PersonId, FirstName, LastName, Age, Gender, Anonymous);
        }
    }
}
