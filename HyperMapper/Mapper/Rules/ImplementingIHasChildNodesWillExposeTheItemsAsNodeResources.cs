using System.Linq;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;

namespace HyperMapper.Mapper.Rules
{
    class ImplementingIHasChildNodesWillExposeTheItemsAsNodeResources : Functions.IAbstractNodeToSemanticDocumentMappingRule
    {
        public virtual void Apply(AbstractNode abstractNode, SemanticDocument doc)
        {
            var childNodes = (abstractNode as IHasChildNodes)?.ChildNodes;
            if (childNodes == null) return;
            ;
            
            foreach (var child in childNodes)
            {
                doc.AddLink(Links.CreateLink(child.Item4, child.Item3, child.Item1));
            }
        }
    }
}
