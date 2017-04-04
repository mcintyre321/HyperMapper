using System;
using HyperMapper.Mapper;
using HyperMapper.Vocab;

namespace HyperMapper.Mapping
{
    public class RootNode : Node
    {
        private readonly Uri _uri;
        public sealed override Uri Uri => _uri;

        public RootNode(string title, Uri uri, Term term) : base(title, term)
        {
            _uri = uri;
            GlossaryNode = new GlossaryNode(this);
        }

        public GlossaryNode GlossaryNode { get; }
    }
}
