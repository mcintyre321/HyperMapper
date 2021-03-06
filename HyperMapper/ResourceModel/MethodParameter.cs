using System;
using System.Collections.Generic;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OneOf;

namespace HyperMapper.ResourceModel
{
    public class MethodParameter
    {
        public MethodParameter(UrlPart urlPart, MethodParameterType type, Term term)
        {
            UrlPart = urlPart;
            Type = type;
            Term = term;
        }

        public MethodParameter(Term term)
        {
            Term = term;
        }

        public UrlPart UrlPart { get; private set; }
        public MethodParameterType Type { get; private set; }
        public Term Term { get; private set; }


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