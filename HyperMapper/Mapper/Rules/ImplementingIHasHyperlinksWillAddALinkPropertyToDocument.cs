using System;
using System.Linq;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;

namespace HyperMapper.Mapper.Rules
{
    class ImplementingIHasHyperlinksWillAddALinkPropertyToDocument : Functions.IAbstractNodeToSemanticDocumentMappingRule
    {
        public virtual void Apply(AbstractNode abstractNode, SemanticDocument doc)
        {

            var hyperlinks = (abstractNode as IHasHyperlinks)?.Hyperlinks ?? Enumerable.Empty<Tuple<Term, Uri, string>>();
            foreach (var hyperlink in hyperlinks)
            {
                doc.AddLink(Links.CreateLink(hyperlink.Item3, hyperlink.Item2, hyperlink.Item1));
            }
        }
    }
}
