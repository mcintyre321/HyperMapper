using System;

namespace HyperMapper.Mapping
{
    public class RelAttribute : Attribute
    {
        private readonly Type _relType;

        public RelAttribute(Type relType)
        {
            _relType = relType;
        }

        public string RelString => _relType.Name.ToString();
    }
}