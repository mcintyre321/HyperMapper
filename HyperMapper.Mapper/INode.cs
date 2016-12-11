using System;
using System.Collections.Generic;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Mapper
{
    public abstract class INode
    {
        public abstract INode Parent { get; }
        public abstract UrlPart UrlPart { get; }
        public abstract IEnumerable<UrlPart> ChildKeys { get; }
        public abstract string Title { get; }
        public abstract bool HasChild(UrlPart urlPart);
        public abstract INode GetChild(UrlPart key);
        public abstract Uri Uri { get; }
        public abstract Term[] Terms { get; }
    }
}