using System.Linq;
using HyperMapper.Vocab;
using OneOf;

namespace HyperMapper.RepresentationModel
{
    public class SemanticDocument : SemanticProperty
    {
        public SemanticDocument() : base(TermFactory.From<SemanticDocument>(), new SemanticPropertiesSet())
        {
        }

        public new SemanticPropertiesSet Value => base.Value.AsT0;

    }
}
