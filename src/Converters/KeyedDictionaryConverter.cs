using System;
using System.Collections.Generic;
using MyCarbonoxidePlugin.Interfaces;
using MyCarbonoxidePlugin.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyCarbonoxidePlugin.Converters;

public class KeyedDictionaryConverter<TValue> : JsonConverter
    where TValue : IKeyed
{
    public override bool CanConvert(Type objectType) => typeof(Dictionary<string, TValue>).IsAssignableFrom(objectType);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        var dict = new Dictionary<string, TValue>(StringComparer.Ordinal);

        foreach (var prop in obj.Properties())
        {
            try
            {
                var value =
                    prop.Value.ToObject<TValue>(serializer)
                    ?? throw new JsonSerializationException($"Deserialized {typeof(TValue).Name} was null.");
                value.Key = prop.Name;
                dict[prop.Name] = value;
            }
            catch (Exception ex)
            {
                var inst = MyCarbonoxide.Instance;
                if (inst is not null)
                {
                    inst.PrintError(
                        $"Skipped {typeof(TValue).Name} '{prop.Name}' due to deserialization error: {ex.Message}\n{ex}"
                    );
                }
                else
                {
                    System.Console.Error.WriteLine(
                        $"[MyCarbonoxide] Data issue: Skipped {typeof(TValue).Name} '{prop.Name}' due to deserialization error: {ex.Message}\n{ex}"
                    );
                }
            }
        }

        return dict;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dict = (Dictionary<string, TValue>)value;

        writer.WriteStartObject();
        foreach (var kvp in dict)
        {
            writer.WritePropertyName(kvp.Key);
            serializer.Serialize(writer, kvp.Value);
        }
        writer.WriteEndObject();
    }
}
