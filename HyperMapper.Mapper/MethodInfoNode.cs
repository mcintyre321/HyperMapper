using System;
using System.Collections.Generic;
using System.Reflection;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Mapper
{
    internal class MethodInfoNode : AbstractNode
    {
        public MethodInfoNode(AbstractNode parent, MethodInfo mi)
        {
            Parent = parent;
            UrlPart = new UrlPart(mi.Name);
            Title = mi.Name;
            MethodInfo = mi;
        }

        public override AbstractNode Parent { get; }
        public override UrlPart UrlPart { get; }
        public override IEnumerable<UrlPart> ChildKeys { get; } = new UrlPart[0];
        public override string Title { get; }
        public override bool HasChild(UrlPart urlPart) => false;

        public override AbstractNode GetChild(UrlPart key) => null;

        public override Uri Uri => UriHelper.Combine(Parent.Uri, UrlPart.ToString());
        public override Term Term => TermFactory.From(MethodInfo);

        public MethodInfo MethodInfo { get; }
    }
}