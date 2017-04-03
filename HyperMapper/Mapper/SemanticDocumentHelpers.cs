using HyperMapper.Helpers;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;

namespace HyperMapper.Mapper
{
    internal static class SemanticDocumentHelpers
    {

        internal static SemanticProperty AddLink(this SemanticDocument doc, SemanticPropertiesSet link)
        {
            var linksRel = TermFactory.From<Links>();
            var links = doc.Value[linksRel] ??
                        (SemanticProperty.CreateList(linksRel, new SemanticPropertiesList())).Then(
                            sp => doc.Value.Add(sp));
            var semanticPropertiesList = links.Value.AsT3;
            semanticPropertiesList.Add(link);
            return links;
        }
    }
}