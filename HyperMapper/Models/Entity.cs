using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.RequestHandling;
using Newtonsoft.Json.Linq;
using OneOf;

namespace HyperMapper.Models
{
   

    public class Entity : IWalkable
    {
        public Key Key { get; set; }

        public Entity(Key key, Uri uri, string[] @class,
            Dictionary<Key, OneOf<SubEntityRef, Action, Link, JToken>> children)
        {
            Key = key;
            Uri = uri;
            Class = @class;
            Children = children;
        }

        public Uri Uri { get; private set; }
      

        public IEnumerable<string> Class { get; private set; }

        public Dictionary<Key, OneOf<SubEntityRef, Action, Link, JToken>> Children
        {
            get;
            private set;
        }
        public IReadOnlyDictionary<Key, SubEntityRef> ChildEntities
        {
            get
            {
                return Children.Where(c => c.Value.IsT0)
                    .ToDictionary(c => c.Key, c => c.Value.AsT0).AsReadOnly();
            }
        }
        public IReadOnlyDictionary<Key, Action> ChildActions
        {
            get
            {
                return Children.Where(c => c.Value.IsT1).ToDictionary(c => c.Key, c => c.Value.AsT1).AsReadOnly();
            }
        }

        public IWalkable Walk(Key key)
        {
            OneOf<SubEntityRef, Action, Link, JToken> child;
            if (!Children.TryGetValue(key, out child)) return null;
            return child.Match<IWalkable>(
                ser => ser.FetchEntity(), 
                action => action, 
                link => null as IWalkable,
                prop => null as IWalkable);

        }
    }
}