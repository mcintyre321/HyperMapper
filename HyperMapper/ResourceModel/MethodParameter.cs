using System;
using System.Collections.Generic;
using HyperMapper.RepresentationModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OneOf;

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

        
        public class MethodParameterType : OneOfBase<MethodParameterType.Text, MethodParameterType.Password, MethodParameterType.Select>
        {
            public class Text : MethodParameterType { }

            public class Password : MethodParameterType { }

            public class Select : MethodParameterType
            {
                public Select(IEnumerable<Option> options)
                {
                    Options = options;
                }

                public IEnumerable<Option> Options { get; }

                public class Option
                {
                    public string Description { get; set; }
                    public string OptionId { get; set; }
                    [JsonIgnore]
                    public object UnderlyingValue { get; set; }
                }
            }
        }
    }


}