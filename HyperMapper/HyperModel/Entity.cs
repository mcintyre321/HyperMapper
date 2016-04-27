using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.RequestHandling;
using OneOf;

namespace HyperMapper.HyperModel
{
   

    public class Entity : IWalkable
    {
        public Entity(Uri uri, string[] @class,
            List<OneOf<SubEntityRef, Action, Link, Property>> children)
        {
            Uri = uri;
            Class = @class;
            Children = children;
        }

        public Uri Uri { get; private set; }
      

        public IEnumerable<string> Class { get; private set; }

        public List<OneOf<SubEntityRef, Action, Link, Property>> Children
        {
            get;
            private set;
        }
        public IEnumerable<SubEntityRef> ChildEntities
        {
            get
            {
                return Children.Where(c => c.IsT0).Select(c => c.AsT0);
            }
        }
        public IEnumerable<Action> ChildActions
        {
            get
            {
                return Children.Where(c => c.IsT1).Select(c => c.AsT1);
            }
        }

        public IWalkable Walk(Key key)
        {
            throw new NotImplementedException();
            OneOf<SubEntityRef, Action, Link, Property> child;
            //if (!Children.TryGetValue(key, out child)) return null;
            //return child.Match<IWalkable>(
            //    ser => ser.FetchEntity(), 
            //    action => action, 
            //    link => null as IWalkable,
            //    prop => null as IWalkable);

        }
    }
}