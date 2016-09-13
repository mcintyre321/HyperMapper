using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HyperMapper.Helpers;
using HyperMapper.Mapper;

namespace HyperMapper.Mapping
{
    /// <summary>
    /// This is an implementation of INode which contains code for adding and removing
    /// well known _children, and automatically attaching to parent. Inherit from this if you are able to,
    /// </summary>
    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class Node : INode
    {
        public override Uri Uri => UriHelper.Combine(Parent.Uri, Key.ToString());

        readonly Dictionary<Key, INode> _children = new Dictionary<Key, INode>();
        public override INode Parent { get; }
        public override Key Key { get; }
        public override IEnumerable<Key> ChildKeys => _children.Keys;
        public override string Title { get; }

        internal Node(string title) 
        {
            Title = title;
            var methodNodes = this.GetType().GetRuntimeMethods()
                .Where(mi => mi.GetCustomAttribute<ExposeAttribute>() != null)
                .Select(rm => new MethodInfoNode(this, rm));
            foreach (var methodNode in methodNodes)
            {
                this.AddChild(methodNode);
            }
        }

        internal Node(INode parent, Key key, string title) : this(title)
        {
            if (parent == null) throw new ArgumentException();
            if (key == null || parent.HasChild(key)) throw new ArgumentException();
            this.Parent = parent;
            this.Key = key;
        }

        public override bool HasChild(Key key)
        {
            return _children.ContainsKey(key.ToString());
        }


        /// <summary>
        /// Return a the child INode which has the matching key. By default this checks the known _children
        /// but can be overridden to return dynamically created _children.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override INode GetChild(Key key)
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
        public T AddChild<T>(T node) where T : INode
        {
            if (node.Parent != this) throw new InvalidOperationException();
            if (this.HasChild(node.Key)) throw new InvalidOperationException();
            _children.Add(node.Key.ToString(), node);
            return node;
        }
    }

    internal class MethodInfoNode : INode
    {
        public MethodInfoNode(INode parent, MethodInfo mi)
        {
            Parent = parent;
            Key = new Key(mi.Name);
            Title = mi.Name;
            MethodInfo = mi;
        }

        public override INode Parent { get; }
        public override Key Key { get; }
        public override IEnumerable<Key> ChildKeys { get; } = new Key[0];
        public override string Title { get; }
        public override bool HasChild(Key key) => false;

        public override INode GetChild(Key key) => null;

        public override Uri Uri => UriHelper.Combine(Parent.Uri, Key.ToString());

        public MethodInfo MethodInfo { get; }
    }

    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class Node<TParent> : Node where TParent : Node
    {
        public new TParent Parent { get; }
        protected Node(TParent parent, Key key, string title) : base(parent, key, title)
        {
            this.Parent = parent;
        }
    }
}
