using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.RequestHandling;
using OneOf;

namespace HyperMapper.HyperModel
{
   

    public class Entity  
    {
        public Entity(Uri uri, string[] @class,
            IEnumerable<OneOf<Entity, SubEntityRef, Action, Link, Property>> children)
        {
            Uri = uri;
            Class = @class;
            Children = children;
        }

        public Uri Uri { get; private set; }
      

        public IEnumerable<string> Class { get; private set; }

        public IEnumerable<OneOf<Entity, SubEntityRef, Action, Link, Property>> Children
        {
            get;
        }

        
 
        public OneOf<Entity, Action, ChildNotFound> GetChildByUriSegment(string part)
        {
            foreach (var child in Children)
            {
                if (child.IsT0 && child.AsT0.Uri.ToString() == this.Uri.ToString().TrimEnd('/') + "/" + part)
                    return child.AsT0;

                if (child.IsT1 && child.AsT1.Uri.ToString() == this.Uri.ToString().TrimEnd('/') + "/" + part)
                    return child.AsT1.FetchEntity();

                    if (child.IsT2 && child.AsT2.Key.ToString() == part) return child.AsT2;
            }

            return new ChildNotFound();
        }

        public class ChildNotFound
        {
        }
    }
}