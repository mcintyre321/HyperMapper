using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.ResourceModel;
using HyperMapper.Vocab;
using Newtonsoft.Json;

namespace HyperMapper.Mapper
{
    public class MethodInfoNode : AbstractNode
    {
        public MethodInfoNode(AbstractNode parent, MethodInfo mi)
        {
            Parent = parent;
            UrlPart = new UrlPart(mi.Name);
            Title = mi.Name;
            MethodInfo = mi;
        }

        public override AbstractNode Parent { get; }
        public override UrlPart UrlPart { get; }
        public override string Title { get; }

        public override Uri Uri => UriHelper.Combine(Parent.Uri, UrlPart.ToString());
        public override Term Term => TermFactory.From(MethodInfo);

        public MethodInfo MethodInfo { get; }

        public MethodParameter[] GetParameters()
        {
            var parameterInfo = this.MethodInfo.GetParameters()
                .Where(mi => mi.GetCustomAttribute<InjectAttribute>() == null)
                .Select(pi => new MethodParameter(pi.Name, GetMethodParameterType(pi), TermFactory.From(pi))).ToArray();

            return parameterInfo.ToArray();

        }

        private MethodParameter.MethodParameterType GetMethodParameterType(ParameterInfo pi)
        {
            var optionsFromAttribute = pi.GetCustomAttribute<OptionsFromAttribute>();
            if (optionsFromAttribute != null)
            {
                var optionsFromMethod =
                    this.Parent.GetType().GetTypeInfo().GetDeclaredMethod(optionsFromAttribute.MethodName);
                var options = (optionsFromMethod.Invoke(this.Parent, new object[0]) as IEnumerable).Cast<object>()
                    .Select(MakeOptionFromValue);

                return new MethodParameter.MethodParameterType.Select(options);
            }
            return new MethodParameter.MethodParameterType.Text();
        }

        internal static MethodParameter.MethodParameterType.Select.Option MakeOptionFromValue(object arg)
        {
            var stringArg = arg as string;
            if (stringArg != null)
                return new MethodParameter.MethodParameterType.Select.Option() { Description = stringArg, OptionId = stringArg };

            return new MethodParameter.MethodParameterType.Select.Option()
            {
                Description = JsonConvert.SerializeObject(arg),
                OptionId = JsonConvert.SerializeObject(arg).GetHashCode().ToString(),
                UnderlyingValue = arg
            };
        }

    }
}