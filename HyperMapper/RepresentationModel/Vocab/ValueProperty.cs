
using Newtonsoft.Json.Linq;

namespace HyperMapper.RepresentationModel.Vocab
{
    public class ValueProperty : Property
    {
        public JToken Value { get; }

        public ValueProperty(string name, JToken value, params Term[] terms) : base(name, terms)
        {
            Value = value;
        }
    }
}