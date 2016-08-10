using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.RequestHandling;
using OneOf;

namespace HyperMapper.HyperModel
{
   

    public class Resource  
    {
        public Resource(Uri uri, string[] @class,
            IEnumerable<OneOf<Operation, Link, Property>> children)
        {
            Uri = uri;
            Class = @class;
            Children = children;
        }

        public Uri Uri { get; private set; }
      

        public IEnumerable<string> Class { get; private set; }

        public IEnumerable<OneOf<Operation, Link, Property>> Children
        {
            get;
        }

        
 
        public OneOf<Resource, Operation, ChildNotFound> GetChildByUriSegment(string part)
        {
            foreach (var child in Children)
            {
                
                if (child.IsT0 && child.AsT0.Href.ToString() == this.Uri.ToString().TrimEnd('/') + "/" + part)
                    return child.AsT0;

                if (child.IsT1 && child.AsT1.Uri.ToString() == this.Uri.ToString().TrimEnd('/') + "/" + part && child.AsT1.Follow != null)
                    return child.AsT1.Follow();

            }

            return new ChildNotFound();
        }

        public class ChildNotFound
        {
        }
    }
}