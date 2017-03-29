using System;
using HyperMapper.RepresentationModel;
using HyperMapper.ResourceModel;
using Newtonsoft.Json.Linq;
using OneOf;

namespace HyperMapper.Vocab
{
    public class Operation
    {
        public static SemanticProperty Create(string title, MethodParameter[] methodParameters, Uri uri, Term term)
        {
            var operationSemProp = new SemanticProperty(term, new SemanticPropertiesSet()
            {
                SemanticProperty.Create<ActionUrl>((JToken) uri.ToString()),
                SemanticProperty.Create<DisplayText>((JToken) title),
            });

            var fields = new SemanticPropertiesSet();
            
            foreach (var methodParameter in methodParameters)
            {
                var fieldProperties = new SemanticPropertiesSet();
                fieldProperties.AddValue<FieldKind>((JToken)methodParameter.Type.Match(text => "text", password => "password", select => "select"));
                fieldProperties.AddValue<FieldName>((JToken)methodParameter.UrlPart.ToString());
                var field = new SemanticProperty(TermFactory.From<Field>(), fieldProperties);
                fields.Add(field);
            }
            operationSemProp.Value.AsT0.Add(new SemanticProperty(TermFactory.From<Fields>(), fields));
            return operationSemProp;
        }
        public class ActionUrl { }

        public class Field
        {
        }
        public class Fields { }
    }
}
