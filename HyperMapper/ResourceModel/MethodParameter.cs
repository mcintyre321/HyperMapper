using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HyperMapper.ResourceModel
{
    public class MethodParameter
    {
        public MethodParameter(Key key, MethodParameterType type)
        {
            Key = key;
            Type = type;
        }

        public MethodParameter() { }

        public Key Key { get; private set; }
        public MethodParameterType Type { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum MethodParameterType
        {
            Text,
            Password
        }
    }


}