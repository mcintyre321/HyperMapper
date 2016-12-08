using System;
using System.Collections.Generic;

namespace HyperMapper.RepresentationModel
{
    public class PropertyList
    {
        HashSet<string> keys = new HashSet<string>();
        private List<Property> _items = new List<Property>();

        public IEnumerable<Property> ToEnumerable()
        {
            return _items;
        }

        public void Add(Property p0)
        {
            if (keys.Add(p0.Name))
            {
                _items.Add(p0);
            }
            else
            {
                throw new Exception($"Already contains property {p0.Name}");
            }
        }
    }
}