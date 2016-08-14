using System.Collections.Generic;

namespace HyperMapper.Mapping
{
    public interface INode
    {
        INode Parent { get; }
        Key Key { get; }
        IEnumerable<Key> ChildKeys { get;  }
        bool HasChild(Key key);
        INode GetChild(Key key);
    }
}