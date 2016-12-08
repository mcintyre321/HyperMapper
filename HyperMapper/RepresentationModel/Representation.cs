using System;
using System.Collections.Generic;
using OneOf;

namespace HyperMapper.RepresentationModel
{
    public class Representation
    {
        public Representation(Uri uri, PropertyList children)
        {
            Uri = uri;
            Children = children.ToEnumerable();
        }

        public Uri Uri { get;  }
        public IEnumerable<Property> Children { get; }
    }
}