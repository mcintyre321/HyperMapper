using System;
using System.Collections.Generic;
using OneOf;

namespace HyperMapper.RepresentationModel
{
    public class Representation
    {
        public Representation(Uri uri, IEnumerable<Property> children)
        {
            Uri = uri;
            Children = children;
        }

        public Uri Uri { get;  }
        public IEnumerable<Property> Children { get; }
    }
}