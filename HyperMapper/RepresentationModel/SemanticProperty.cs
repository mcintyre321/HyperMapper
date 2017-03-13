using System;
using HyperMapper.Vocab;
using Newtonsoft.Json.Linq;
using OneOf;

namespace HyperMapper.RepresentationModel
{
    public class SemanticProperty
    {
        public static SemanticProperty<SemanticPropertiesSet> CreateSet(Term term, SemanticPropertiesSet value = null) { return new SemanticProperty<SemanticPropertiesSet>(term, value); }
        public static SemanticProperty<SemanticPropertiesSet> CreateSet<TTerm>(SemanticPropertiesSet value = null) { return CreateSet(TermFactory.From<TTerm>(), value); }

        public static SemanticProperty<JToken> CreateValue(Term term, JToken value) { return new SemanticProperty<JToken>(term, value); }
        public static SemanticProperty<JToken> CreateValue<T>(JToken value) { return CreateValue(TermFactory.From<T>(), value); }

        public static SemanticProperty<Term> CreateTerm(Term term, Term value) { return new SemanticProperty<Term>(term, value); }
        public static SemanticProperty<Term> CreateTerm<TTerm>(Term value) { return CreateTerm(TermFactory.From<TTerm>(), value); }
        public static SemanticProperty<Term> CreateTerm<TTerm, TValueTerm>() { return CreateTerm(TermFactory.From<TTerm>(), TermFactory.From<TValueTerm>()); }


        public static SemanticProperty<SemanticPropertiesList> CreateList(Term term, SemanticPropertiesList value) { return new SemanticProperty<SemanticPropertiesList>(term, value); }
        public static SemanticProperty<SemanticPropertiesList> CreateList<TTerm>(SemanticPropertiesList value) { return CreateList(TermFactory.From<TTerm>(), value); }

        public Term Term { get; set; }

        protected SemanticProperty(Term term, OneOf<SemanticPropertiesSet, JToken, Term, SemanticPropertiesList> value)
        {
            if (term == null) throw new ArgumentNullException(nameof(term));
            Term = term;
            Value = value;
        }

        public OneOf<SemanticPropertiesSet, JToken, Term, SemanticPropertiesList> Value { get; }

        public static SemanticProperty Create<T>(OneOf<SemanticPropertiesSet, JToken, Term, SemanticPropertiesList> value)
        {
            return new SemanticProperty(TermFactory.From<T>(), value);

        }
    }
    public class SemanticProperty<TValue> : SemanticProperty  
    {

        internal SemanticProperty(Term term, SemanticPropertiesSet value) : base(term, value) { }
        internal SemanticProperty(Term term, JToken value) : base(term, value) { }
        internal SemanticProperty(Term term, Term value) : base(term, value) { }
        internal SemanticProperty(Term term, SemanticPropertiesList value) : base(term, value)  { }

        public TValue Value => (TValue) ((IOneOf) base.Value).Value;
    }
}