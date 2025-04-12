using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Edulink.Converters
{
    public class IPEndPointConverter : JsonConverter<IPEndPoint>
    {
        public override IPEndPoint ReadJson(JsonReader reader, Type objectType, IPEndPoint existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            IPAddress address = IPAddress.Parse(jObject["Address"].Value<string>());
            int port = jObject["Port"].Value<int>();

            return new IPEndPoint(address, port);
        }

        public override void WriteJson(JsonWriter writer, IPEndPoint value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Address");
            writer.WriteValue(value.Address.ToString());
            writer.WritePropertyName("Port");
            writer.WriteValue(value.Port);
            writer.WriteEndObject();
        }
    }
}
