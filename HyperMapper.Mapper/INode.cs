using System;
using System.Collections.Generic;

namespace HyperMapper.Mapping
{
    public abstract class INode
    {
        public abstract INode Parent { get; }
        public abstract Key Key { get; }
        public abstract IEnumerable<Key> ChildKeys { get; }
        public abstract string Title { get; }
        public abstract bool HasChild(Key key);
        public abstract INode GetChild(Key key);
        public abstract Uri Uri { get; }
    }
}