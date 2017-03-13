using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.Vocab;
using Newtonsoft.Json.Linq;
using OneOf;

namespace HyperMapper.RepresentationModel
{
    public class SemanticPropertiesSet : IEnumerable<SemanticProperty>
    {
        public Dictionary<Term, SemanticProperty> _items = new Dictionary<Term, SemanticProperty>();

        public SemanticProperty this[Term term]
        {
            get
            {
                var sp = null as SemanticProperty;
                _items.TryGetValue(term, out sp);
                return sp;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<SemanticProperty> GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        public void Add(SemanticProperty semanticProperty)
        {
            _items.Add(semanticProperty.Term, semanticProperty);
        }
 
    }
    public class SemanticPropertiesList : IEnumerable<OneOf<SemanticPropertiesSet, JToken, Term, SemanticPropertiesList>>
    {
        public List<OneOf<SemanticPropertiesSet, JToken, Term, SemanticPropertiesList>> _items = new List<OneOf<SemanticPropertiesSet, JToken, Term, SemanticPropertiesList>>();


        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<OneOf<SemanticPropertiesSet, JToken, Term, SemanticPropertiesList>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public void Add(OneOf<SemanticPropertiesSet, JToken, Term, SemanticPropertiesList> value)
        {
            _items.Add(value);
        }
    }
}