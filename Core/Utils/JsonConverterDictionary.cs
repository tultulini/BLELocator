using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BLELocator.Core.Utils
{
    public class JsonConverterDictionary<TKey, TValue> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dictionary = value as Dictionary<TKey, TValue>;
            if (dictionary == null)
            {
                writer.WriteNull();
                return;
            }
            List<KeyValuePair<TKey, TValue>> pairs = dictionary.ToList();
            serializer.Serialize(writer, pairs);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var concreteType = Activator.CreateInstance(objectType);
            var dictionary = (Dictionary<TKey, TValue>)concreteType;
            var pairs = serializer.Deserialize<List<KeyValuePair<TKey, TValue>>>(reader);
            foreach (var keyValuePair in pairs)
            {
                if (dictionary.ContainsKey(keyValuePair.Key))
                    continue;
                dictionary.Add(keyValuePair.Key, keyValuePair.Value);


            }
            return concreteType;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Dictionary<TKey, TValue>) == objectType || typeof(Dictionary<TKey, TValue>) == objectType.BaseType;
        }

    }
}