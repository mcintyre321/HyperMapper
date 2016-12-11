using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HyperMapper.ResourceModel
{
    public class MethodParameter
    {
        public MethodParameter(UrlPart urlPart, MethodParameterType type)
        {
            UrlPart = urlPart;
            Type = type;
        }

        public MethodParameter() { }

        public UrlPart UrlPart { get; private set; }
        public MethodParameterType Type { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum MethodParameterType
        {
            Text,
            Password
        }
    }


}