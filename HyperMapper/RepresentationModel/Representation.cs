using System;
using System.Collections.Generic;
using OneOf;

namespace HyperMapper.RepresentationModel
{
    public class Representation
    {
        public Representation(IEnumerable<Class> @class, string title, Uri uri, IEnumerable<OneOf<Link, Property, Operation>> children)
        {
            Class = @class;
            Title = title;
            Uri = uri;
            Children = children;
        }

        public IEnumerable<Class> Class { get; }
        public string Title { get; set; }
        public Uri Uri { get;  }
        public IEnumerable<OneOf<Link, Property, Operation>> Children { get; }
    }
}