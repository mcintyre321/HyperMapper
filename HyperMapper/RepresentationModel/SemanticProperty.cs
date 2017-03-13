using System;
using HyperMapper.Vocab;
using Newtonsoft.Json.Linq;
using OneOf;

namespace HyperMapper.RepresentationModel
{
    public class SemanticProperty
    {
        public Term Term { get; set; }

        public SemanticProperty(Term term, OneOf.OneOf<SemanticPropertiesSet, JToken, Term, SemanticPropertiesList> value)
        {
            if (term == null) throw new ArgumentNullException(nameof(term));
            Term = term;
            Value = value;
        }

        public OneOf<SemanticPropertiesSet, JToken, Term, SemanticPropertiesList> Value { get; }

    }
}