using System;
using System.Collections.Generic;
using HyperMapper.Mapper;

namespace HyperMapper.Mapping
{
    public sealed class ChildNodes
    {
        private readonly AbstractNode _this;
        readonly Dictionary<UrlPart, AbstractNode> items = new Dictionary<UrlPart, AbstractNode>();
        public ChildNodes(AbstractNode @this)
        {
            _this = @this;
        }

        public T AddChild<T>(T node) where T : AbstractNode
        {
            if (node.Parent != _this) throw new InvalidOperationException();
            if (items.ContainsKey(node.UrlPart)) throw new InvalidOperationException();
            items.Add(node.UrlPart.ToString(), node);
            return node;
        }
    }
}