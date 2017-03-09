using System;
using System.Collections.Generic;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Mapper
{
    public abstract class AbstractNode
    {
        public abstract AbstractNode Parent { get; }
        public abstract UrlPart UrlPart { get; }
        public abstract IEnumerable<UrlPart> ChildKeys { get; }
        public abstract string Title { get; }
        public abstract bool HasChild(UrlPart urlPart);
        public abstract AbstractNode GetChild(UrlPart key);
        public abstract Uri Uri { get; }
        public abstract Term Term { get; }

    }
}