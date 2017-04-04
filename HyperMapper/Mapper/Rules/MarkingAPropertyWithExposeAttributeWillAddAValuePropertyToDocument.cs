using System.Linq;
using System.Reflection;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;
using Newtonsoft.Json.Linq;

namespace HyperMapper.Mapper.Rules
{
    public class MarkingAPropertyWithExposeAttributeWillAddAValuePropertyToDocument : Functions.IAbstractNodeToSemanticDocumentMappingRule
    {
        public virtual void Apply(AbstractNode abstractNode, SemanticDocument doc)
        {
            var type = abstractNode.GetType().GetTypeInfo();
            var markedUpProps = type.DeclaredProperties
                .Select(propertyInfo => new
                {
                    propertyInfo = propertyInfo,
                    att = CustomAttributeExtensions.GetCustomAttribute<ExposeAttribute>((MemberInfo) propertyInfo),
                    propertyUri = UriHelper.Combine(abstractNode.Uri, propertyInfo.Name)
                }).Where(x => x.att != null);


            foreach (var x in markedUpProps)
            {
                if (typeof(AbstractNode).GetTypeInfo().IsAssignableFrom(x.propertyInfo.PropertyType.GetTypeInfo()))
                {
                    var childNode = (AbstractNode)x.propertyInfo.GetValue(abstractNode);
                    doc.AddLink(Links.CreateLink(childNode.Title, childNode.Uri, childNode.Term));
                }
                else
                {
                    doc.Value.Add(SemanticProperty.CreateValue(TermFactory.From(x.propertyInfo),
                        JToken.FromObject(x.propertyInfo.GetValue(abstractNode))));
                }
            }
        }

    }
}