using System;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomerRecognition.Common
{
    public class RectangleJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Rectangle));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Rectangle rect = (Rectangle)value;
            JObject jo = new JObject();
            jo.Add("x", rect.X);
            jo.Add("y", rect.Y);
            jo.Add("width", rect.Width);
            jo.Add("height", rect.Height);
            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            return new Rectangle((int)jo["x"], (int)jo["y"], (int)jo["width"], (int)jo["height"]);
        }
    }
}