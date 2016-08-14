using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.Helpers;

namespace HyperMapper.Mapping
{
    /// <summary>
    /// This is an implementation of INode which contains code for adding and removing
    /// well known _children, and automatically attaching to parent. Inherit from this if you are able to,
    /// </summary>
    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class Node : INode
    {
        readonly Dictionary<Key, INode> _children = new Dictionary<Key, INode>();
        public INode Parent { get; }
        public Key Key { get; }
        public IEnumerable<Key> ChildKeys => _children.Keys;

        internal Node() { }
        internal Node(INode parent, Key key)
        {
            if (parent == null) throw new ArgumentException();
            if (key == null || parent.HasChild(key)) throw new ArgumentException();
            this.Parent = parent;
            this.Key = key;
        }

        public virtual bool HasChild(Key key)
        {
            return _children.ContainsKey(key.ToString());
        }


        /// <summary>
        /// Return a the child INode which has the matching key. By default this checks the known _children
        /// but can be overridden to return dynamically created _children.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual INode GetChild(Key key)
        {
            INode node = null;
            if (_children.TryGetValue(key, out node)) return node;
            return null;
        }

        private INode Root => ((INode)this).Recurse(n => n.Parent).TakeWhile(n => n != null).Last();

        /// <summary>
        /// Add a node to the known _children of this Node, so that the node can be found by it's Key
        /// property, and can be found. This is as opposed to the 'unknown' _children e.g. a child that
        /// can only be accessed via an expense query.
        /// 
        /// This will not allow you to add a child if the Key is already a known child. The node being added
        /// must already have this node set as it's parent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns>The child, for fluent method chaining</returns>
        public T AddChild<T>(T node) where T : Node
        {
            if (node.Parent != this) throw new InvalidOperationException();
            if (this.HasChild(node.Key)) throw new InvalidOperationException();
            _children.Add(node.Key.ToString(), node);
            return node;
        }
    }

    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class Node<TParent> : Node where TParent : Node
    {
        public new TParent Parent { get; }
        protected Node(TParent parent, Key key) : base(parent, key)
        {
            this.Parent = parent;
        }
    }


    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class Node<TValue, TParent> : Node<TParent> where TParent : Node
    {
        public TValue Value { get; }
        protected Node(TValue value, TParent parent, Key key) : base(parent, key)
        {
            this.Value = value;
        }

        public static implicit operator TValue(Node<TValue, TParent> node)
        {
            return node.Value;
        }
    }
}
