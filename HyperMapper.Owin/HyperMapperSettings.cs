using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace HyperMapper.Owin
{
    public class HyperMapperSettings
    {
        public HyperMapperSettings()
        {
            JsonSerializerSettings = GetJsonSerializerSettings();
        }

        public string BasePath { get; set; }
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public Func<Type, object> ServiceLocator { get; set; }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            };
            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            return settings;
        }
    }
}