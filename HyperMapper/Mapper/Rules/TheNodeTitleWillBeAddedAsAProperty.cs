using System;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;
using Newtonsoft.Json.Linq;

namespace HyperMapper.Mapper.Rules
{
    public class TheNodeTitleWillBeAddedAsAProperty : Functions.IAbstractNodeToSemanticDocumentMappingRule
    {
        public void Apply(AbstractNode node, SemanticDocument doc)
        {
            doc.Value.Add(SemanticProperty.CreateValue(Terms.Title, JToken.FromObject(node.Title)));

        }
    }
}