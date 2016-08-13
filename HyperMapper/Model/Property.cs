using Newtonsoft.Json.Linq;

namespace HyperMapper.Model
{
    public class Property
    {
        public string Name { get;  }
        public JToken Value { get; }

        public Property(string name, JToken value)
        {
            Name = name;
            Value = value;
        }
    }
}