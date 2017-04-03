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
            var operationSemProp = SemanticProperty.CreateSet(term, new SemanticPropertiesSet()
            {
                SemanticProperty.Create<ActionUrl>((JToken) uri.ToString()),
                SemanticProperty.Create<DisplayText>((JToken) title),
            });

            var fields = new SemanticPropertiesSet();
            
            foreach (var methodParameter in methodParameters)
            {
                var fieldProperties = new SemanticPropertiesSet();
                methodParameter.Type.Switch(
                    text => fieldProperties.Add(SemanticProperty.CreateTerm<FieldKind, TextField>()),
                    password => fieldProperties.Add(SemanticProperty.CreateTerm<FieldKind, PasswordField>()),
                    select =>
                    {
                        fieldProperties.Add(SemanticProperty.CreateTerm<FieldKind, SelectField>());
                        var optionsList = new SemanticPropertiesList();
                        foreach (var selectOption in select.Options)
                        {
                            optionsList.Add(new SemanticPropertiesSet
                            {
                                SemanticProperty.CreateValue<DisplayText>(selectOption.Description),
                                SemanticProperty.CreateValue<FieldValue>(selectOption.OptionId)
                            });
                        }
                        var options = SemanticProperty.CreateList<Options>(optionsList);

                        fieldProperties.Add(options);
                    }
                );
                fieldProperties.Add(SemanticProperty.CreateValue<FieldName>((JToken)methodParameter.UrlPart.ToString()));
                var field = SemanticProperty.CreateSet(methodParameter.Term, fieldProperties);
                fields.Add(field);
            }
            operationSemProp.Value.Add(SemanticProperty.CreateSet(TermFactory.From<Fields>(), fields));
            return operationSemProp;
        }

        public class FieldKind
        {
            
        }
        public class TextField { }
        public class PasswordField { }
        public class SelectField { }


        public class ActionUrl { }
        public class FieldValue { }

        public class Options { }
        public class Fields { }
    }
}
