using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HyperMapper.Helpers;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Mapper
{
    /// <summary>
    /// This is an implementation of INode which contains code for adding and removing
    /// well known _children, and automatically attaching to parent. Inherit from this if you are able to,
    /// </summary>
    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class Node : AbstractNode
    {
        public override Uri Uri => UriHelper.Combine(Parent.Uri, UrlPart.ToString());
        public override Term Term { get; }

        readonly Dictionary<UrlPart, AbstractNode> _children = new Dictionary<UrlPart, AbstractNode>();
        public override AbstractNode Parent { get; }
        public override UrlPart UrlPart { get; }
        public override IEnumerable<UrlPart> ChildKeys => _children.Keys;
        public override string Title { get; }

        internal Node(string title, Term term) 
        {
            Title = title;
            Term = term;
            var methodNodes = this.GetType().GetRuntimeMethods()
                .Where(mi => mi.GetCustomAttribute<ExposeAttribute>() != null)
                .Select(rm => new MethodInfoNode(this, rm));
            foreach (var methodNode in methodNodes)
            {
                this.AddChild(methodNode);
            }
        }

        protected internal Node(AbstractNode parent, UrlPart urlPart, string title, Term terms) : this(title, terms)
        {
            if (parent == null) throw new ArgumentException();
            if (urlPart == null || parent.HasChild(urlPart)) throw new ArgumentException();
            this.Parent = parent;
            this.UrlPart = urlPart;
        }

        public override bool HasChild(UrlPart urlPart)
        {
            return _children.ContainsKey(urlPart.ToString());
        }


        /// <summary>
        /// Return a the child INode which has the matching UrlPart. By default this checks the known _children
        /// but can be overridden to return dynamically created _children.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override AbstractNode GetChild(UrlPart key)
        {
            AbstractNode node = null;
            if (_children.TryGetValue(key, out node)) return node;
            return null;
        }

        private AbstractNode Root => ((AbstractNode)this).Recurse(n => n.Parent).TakeWhile(n => n != null).Last();

        /// <summary>
        /// Add a node to the known _children of this Node, so that the node can be found by it's UrlPart
        /// property, and can be found. This is as opposed to the 'unknown' _children e.g. a child that
        /// can only be accessed via an expense query.
        /// 
        /// This will not allow you to add a child if the UrlPart is already a known child. The node being added
        /// must already have this node set as it's parent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns>The child, for fluent method chaining</returns>
        public T AddChild<T>(T node) where T : AbstractNode
        {
            if (node.Parent != this) throw new InvalidOperationException();
            if (this.HasChild(node.UrlPart)) throw new InvalidOperationException();
            _children.Add(node.UrlPart.ToString(), node);
            return node;
        }
    }

    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class Node<TParent> : Node where TParent : Node
    {
        public new TParent Parent { get; }
        protected Node(TParent parent, UrlPart urlPart, string title, Term terms) : base(parent, urlPart, title, terms)
        {
            this.Parent = parent;
        }
    }
}
