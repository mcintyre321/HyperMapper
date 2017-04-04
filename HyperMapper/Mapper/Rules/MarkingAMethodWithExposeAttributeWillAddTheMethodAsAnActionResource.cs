using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;

namespace HyperMapper.Mapper.Rules
{
    public class MarkingAMethodWithExposeAttributeWillAddTheMethodAsAnActionResource : Functions.IAbstractNodeToSemanticDocumentMappingRule
    {
        public virtual void Apply(AbstractNode abstractNode, SemanticDocument doc)
        {
            var operations = GetOperations(abstractNode.GetType().GetTypeInfo(), abstractNode);
            foreach (var linkedActionResource in operations)
            {
                doc.Value.Add(linkedActionResource);
            }
        }

        internal static IEnumerable<SemanticProperty> GetOperations(TypeInfo type, AbstractNode node)
        {
            return type.DeclaredMethods.Where(x => x.GetCustomAttributes<ExposeAttribute>().Any())
                .Select(mi => new MethodInfoNode(node, mi))
                .Select(min => Operation.Create(min.Title, min.GetParameters(), min.Uri, min.Term));
        }
    }
}