using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace PIA.Model
{
    public class IngredientesConverter : JsonConverter<List<string>>
    {
        public override List<string> ReadJson(JsonReader reader, Type objectType, List<string> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var ingredientes = new List<string>();

            if (reader.TokenType == JsonToken.StartArray)
            {
                ingredientes = serializer.Deserialize<List<string>>(reader);
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                var jsonObject = JObject.Load(reader);
                foreach (var property in jsonObject.Properties())
                {
                    ingredientes.Add(property.Value.ToString());
                }
            }
            else
            {
                throw new JsonSerializationException("Unexpected token type when deserializing Ingredientes");
            }

            return ingredientes;
        }

        public override void WriteJson(JsonWriter writer, List<string> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
