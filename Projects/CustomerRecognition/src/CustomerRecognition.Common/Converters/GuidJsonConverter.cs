using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerRecognition.Common
{
	public class GuidJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(Guid) == objectType;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			switch (reader.TokenType)
			{
				case JsonToken.Null:
					return Guid.Empty;
				case JsonToken.String:
					string str = reader.Value as string;
					if (string.IsNullOrEmpty(str))
					{
						return Guid.Empty;
					}
					else
					{
						return new Guid(str);
					}
				default:
					throw new ArgumentException("Invalid token type");
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (Guid.Empty.Equals(value))
			{
				writer.WriteValue("");
			}
			else
			{
				writer.WriteValue((Guid)value);
			}
		}
	}
}
