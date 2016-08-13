using System;
using System.Collections.Generic;
using OneOf;

namespace HyperMapper.HyperModel
{
    public class Representation
    {
        public Representation(IEnumerable<string> @class, Uri uri, IEnumerable<OneOf<Link, Property>> children)
        {
            Class = @class;
            Uri = uri;
            Children = children;
        }

        public IEnumerable<string> Class { get; }
        public Uri Uri { get;  }
        public IEnumerable<OneOf<Link, Property>> Children { get; }
    }
}