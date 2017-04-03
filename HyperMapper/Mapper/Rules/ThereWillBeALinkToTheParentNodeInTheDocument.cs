using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;

namespace HyperMapper.Mapper.Rules
{
    public class ThereWillBeALinkToTheParentNodeInTheDocument : Functions.IAbstractNodeToSemanticDocumentMappingRule
    {
        public void Apply(AbstractNode node, SemanticDocument doc)
        {
            if (node.Parent != null)
            {
                doc.AddLink(Links.CreateLink(node.Parent.Title, node.Parent.Uri, Terms.Parent));
            }

        }
    }
}